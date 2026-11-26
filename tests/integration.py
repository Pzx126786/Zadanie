"""Testy HTTP: odizolowana baza SQLite, prawdziwy ASP.NET Core MVC. Python 3, bez pakietów."""
import http.cookiejar, urllib.request, urllib.parse, urllib.error, re, subprocess, tempfile, os, time, sqlite3, pathlib, html, json
ROOT=pathlib.Path(__file__).resolve().parents[1]
DLL=ROOT/'bin/Debug/net8.0/BeFit.dll'
PORT=5087
BASE=f'http://localhost:{PORT}'
checks=0
class Client:
 def __init__(self):
  self.opener=urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
 def call(self,path,data=None):
  req=urllib.request.Request(BASE+path, None if data is None else urllib.parse.urlencode(data).encode())
  try:
   with self.opener.open(req) as r:return r.status,r.read().decode(),r.url
  except urllib.error.HTTPError as e:return e.code,e.read().decode(),e.url
 def post(self,path,data,token_path=None):
  status,body,url=self.call(token_path or path)
  m=re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"',body)
  assert m,(status,path,body[:500])
  return self.call(path,dict(data,__RequestVerificationToken=html.unescape(m[1])))
def check(ok,name):
 global checks
 assert ok,name
 checks+=1;print('OK',name,flush=True)
def register(c,email):
 return c.post('/Account/Register',{'Email':email,'Password':'TestHaslo12345','ConfirmPassword':'TestHaslo12345'})
def session(c,name='Trening testowy',start='2026-11-25T10:00',end='2026-11-25T11:00',extra=None):
 data={'Name':name,'Start':start,'End':end,'Notes':'Notatka'};data.update(extra or {})
 return c.post('/TrainingSessions/Create',data)
import contextlib, uuid
test_dir=ROOT/'tests'/('run-'+uuid.uuid4().hex)
test_dir.mkdir()
with contextlib.nullcontext(str(test_dir)) as tmp:
 env=dict(os.environ,ConnectionStrings__DefaultConnection=f'Data Source={tmp}/test.db',ASPNETCORE_ENVIRONMENT='Development',ASPNETCORE_URLS=BASE,DOTNET_CLI_HOME=str(ROOT/'.local-dotnet'))
 with open(pathlib.Path(tmp)/'server.log','w',encoding='utf-8') as log:
  proc=subprocess.Popen(['dotnet',str(DLL)],cwd=ROOT,env=env,stdout=log,stderr=log)
  try:
   a,b,guest=Client(),Client(),Client()
   for i in range(100):
    try:
     if guest.call('/')[0]==200:break
    except OSError:pass
    if proc.poll() is not None:raise Exception('Server stopped; '+(pathlib.Path(tmp)/'server.log').read_text())
    time.sleep(.1)
   check(guest.call('/ExerciseTypes')[0]==200,'Katalog publiczny')
   check('/Account/Login' in guest.call('/TrainingSessions')[2],'Treningi wymagaja logowania')
   check(guest.call('/App_Data/befit.db')[0]==404,'Baza nie jest publiczna')
   check(register(a,'a@example.pl')[0]==200,'Rejestracja pierwszego konta')
   check(register(b,'b@example.pl')[0]==200,'Rejestracja drugiego konta')
   check(a.call('/ExerciseTypes/Create')[0]==403,'Pierwsze konto nie otrzymuje samoczynnie roli')
   check(a.call('/TrainingSessions/Create',{'Name':'Bez tokena'})[0]==400,'Ochrona CSRF')
   check('validation-summary-errors' in session(a,start='2026-11-25T12:00')[1],'Koniec przed poczatkiem odrzucony')
   status,body,url=session(a,extra={'UserId':'podmieniony','Id':'999'})
   check(status==200 and '/Details/' in url,'Utworzenie treningu')
   sid=int(url.rsplit('/',1)[1])
   with sqlite3.connect(pathlib.Path(tmp)/'test.db') as db:
    uid=db.execute('SELECT Id FROM AspNetUsers WHERE Email=?',('a@example.pl',)).fetchone()[0]
    check(db.execute('SELECT UserId FROM TrainingSessions WHERE Id=?',(sid,)).fetchone()[0]==uid,'Wlasciciel przypisany przez serwer')
   for action in ['Details','Edit','Delete']:
    check(b.call(f'/TrainingSessions/{action}/{sid}')[0]==404,f'Izolacja treningu {action}')
   check(b.post(f'/TrainingSessions/Edit/{sid}',{'Name':'Atak','Start':'2026-11-25T10:00','End':'2026-11-25T11:00'},'/TrainingSessions/Create')[0]==404,'Blokada edycji cudzego treningu')
   check(b.post(f'/TrainingSessions/Delete/{sid}',{},'/TrainingSessions/Create')[0]==404,'Blokada usuniecia cudzego treningu')
   data={'TrainingSessionId':sid,'ExerciseTypeId':1,'Weight':'12,5','Sets':3,'Repetitions':10}
   status,body,url=a.post('/PerformedExercises/Create',data)
   check(status==200 and '12,5 kg' in body,'Zapis wykonania z polskim separatorem')
   with sqlite3.connect(pathlib.Path(tmp)/'test.db') as db:eid=db.execute('SELECT Id FROM PerformedExercises').fetchone()[0]
   for action in ['Details','Edit','Delete']:
    check(b.call(f'/PerformedExercises/{action}/{eid}')[0]==404,f'Izolacja wykonania {action}')
   check(b.post('/PerformedExercises/Create',data)[0]==404,'Nie mozna dopisac cwiczenia do cudzego treningu')
   check(b.post(f'/PerformedExercises/Edit/{eid}',data,'/PerformedExercises/Create')[0]==404,'Blokada edycji cudzego wykonania')
   check(b.post(f'/PerformedExercises/Delete/{eid}',{},'/PerformedExercises/Create')[0]==404,'Blokada usuniecia cudzego wykonania')
   check('validation-summary-errors' in a.post('/PerformedExercises/Create',dict(data,Sets=0))[1],'Nieprawidlowa liczba serii')
   check('validation-summary-errors' in a.post('/PerformedExercises/Create',dict(data,Weight='12,555'))[1],'Precyzja obciazenia')
   check(a.post(f'/PerformedExercises/Edit/{eid}',dict(data,Weight='20',Sets=4))[0]==200,'Edycja wykonania')
   # Window uses actual local now; set deterministic relative dates directly in isolated DB.
   import datetime
   now=datetime.datetime.now()
   with sqlite3.connect(pathlib.Path(tmp)/'test.db') as db:
    db.execute('UPDATE TrainingSessions SET Start=?, End=? WHERE Id=?',((now-datetime.timedelta(hours=2)).isoformat(' '),(now-datetime.timedelta(hours=1)).isoformat(' '),sid))
   a.post('/PerformedExercises/Create',dict(data,Weight='40',Sets=2,Repetitions=5))
   body=a.call('/Statistics')[1]
   check('30 kg' in body and '40 kg' in body and '<td>50</td>' in body,'Statystyki: srednia, maksimum, serie razy powtorzenia')
   check('40 kg' not in b.call('/Statistics')[1],'Statystyki tylko wlasciciela')
   check('<td>0</td><td>0</td>' in body,'Statystyki zawieraja niewykonane typy')
   zero=dict(data,ExerciseTypeId=2,Weight='0')
   a.post('/PerformedExercises/Create',zero)
   check('0 kg' in a.call('/Statistics')[1],'Zerowe obciazenie nie jest brakiem danych')
   proc.terminate();proc.wait(timeout=15)
   proc=subprocess.Popen(['dotnet',str(DLL)],cwd=ROOT,env=env,stdout=log,stderr=log)
   for i in range(100):
    try:
     if guest.call('/')[0]==200:break
    except OSError:pass
    time.sleep(.1)
   check('40 kg' in a.call('/Statistics')[1],'Baza i sesja logowania trwale po restarcie')
   with sqlite3.connect(pathlib.Path(tmp)/'test.db') as db:
    db.execute('UPDATE TrainingSessions SET Start=? WHERE Id=?',((now-datetime.timedelta(days=29)).isoformat(' '),sid))
   check('40 kg' not in a.call('/Statistics')[1],'Statystyki pomijaja starsze niz 28 dni')
   with sqlite3.connect(pathlib.Path(tmp)/'test.db') as db:
    db.execute('UPDATE TrainingSessions SET Start=? WHERE Id=?',((now+datetime.timedelta(days=1)).isoformat(' '),sid))
   check('40 kg' not in a.call('/Statistics')[1],'Statystyki pomijaja przyszle treningi')
   # Exercise catalog administrator uses the same local command delivered to the operator.
   result=subprocess.run(['dotnet',str(DLL),'--make-admin','a@example.pl'],cwd=ROOT,env=env,capture_output=True)
   check(result.returncode==0,'Lokalne nadanie roli administratora')
   a.post('/Account/Logout',{},'/TrainingSessions/Create')
   a.post('/Account/Login',{'Email':'a@example.pl','Password':'TestHaslo12345'})
   check(a.call('/ExerciseTypes/Create')[0]==200,'Administrator moze zarzadzac katalogiem')
   _,_,other_url=session(b,name='Cudzy trening')
   other_id=int(other_url.rsplit('/',1)[1])
   check(a.call(f'/TrainingSessions/Details/{other_id}')[0]==404,'Administrator nie widzi cudzych treningow')
   check(a.post(f'/TrainingSessions/Edit/{sid}',{'Name':'Zmieniony trening','Start':'2026-11-25T10:00','End':'2026-11-25T11:00','Notes':'Edycja'})[0]==200,'Edycja treningu')
   a.post('/ExerciseTypes/Create',{'Name':'Nowe cwiczenie','Description':'Opis'})
   with sqlite3.connect(pathlib.Path(tmp)/'test.db') as db:tid=db.execute('SELECT Id FROM ExerciseTypes WHERE Name=?',('Nowe cwiczenie',)).fetchone()[0]
   check(a.post(f'/ExerciseTypes/Edit/{tid}',{'Name':'Zmienione cwiczenie','Description':'Nowy opis'})[0]==200,'CRUD typu: tworzenie i edycja')
   check('validation-summary-errors' in a.post('/ExerciseTypes/Delete/1',{})[1],'Nie mozna usunac uzywanego typu')
   check(a.post(f'/ExerciseTypes/Delete/{tid}',{})[0]==200,'CRUD typu: usuniecie')
   check(a.post(f'/PerformedExercises/Delete/{eid}',{})[0]==200,'Usuniecie wykonania')
   check(a.call(f'/PerformedExercises/Details/{eid}')[0]==404,'Wykonanie usuniete')
   a.post('/PerformedExercises/Create',data)
   check(a.post(f'/TrainingSessions/Delete/{sid}',{})[0]==200,'Usuniecie treningu')
   with sqlite3.connect(pathlib.Path(tmp)/'test.db') as db:check(db.execute('SELECT count(*) FROM PerformedExercises').fetchone()[0]==0,'Kaskadowe usuniecie wykonan')
   a.post('/Account/Logout',{},'/TrainingSessions/Create')
   check('/Account/Login' in a.call('/Statistics')[2],'Wylogowanie odbiera dostep')
   print(f'PASSED: {checks} checks')
  finally:
   proc.terminate();proc.wait(timeout=15)

