# BeFit — instrukcja obsługi

BeFit służy do zapisywania treningów i sprawdzania swoich wyników. Poniżej opisano, jak uruchomić aplikację na komputerze z Windows.

## 1. Rozpakuj aplikację

1. Kliknij prawym przyciskiem myszy plik `.zip`.
2. Wybierz **Wyodrębnij wszystkie**, a następnie **Wyodrębnij**.
3. Otwórz rozpakowany folder **BeFit**. Znajdziesz w nim plik `Uruchom.cmd`.

Nie uruchamiaj aplikacji bezpośrednio ze środka ZIP-a.

## 2. Uruchom BeFit

1. Połącz komputer z internetem. Przy pierwszym uruchomieniu mogą zostać pobrane potrzebne pliki.
2. Kliknij dwa razy plik **Uruchom.cmd**. Jeśli Windows ukrywa końcówki nazw plików, zobaczysz po prostu **Uruchom**.
3. Otworzy się okno z napisami. Poczekaj, aż pojawi się tekst **Now listening on** z adresem strony. Pierwsze uruchomienie może potrwać kilka minut.
4. Nie zamykaj tego okna. Możesz je zminimalizować.
5. Otwórz przeglądarkę, np. Edge lub Chrome.
6. W pasku adresu u góry wpisz **http://localhost:5080** i naciśnij **Enter**.

Powinna otworzyć się strona BeFit.

### Jeśli aplikacja się nie uruchamia

Jeśli okno informuje o braku programu **dotnet** lub **.NET SDK**:

1. Otwórz [stronę pobierania Microsoft .NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).
2. W części **Build apps — SDK** wybierz instalator dla swojego Windows. Na większości komputerów będzie to **Windows x64**.
3. Otwórz pobrany plik i przejdź przez instalację.
4. Zamknij poprzednie okno BeFit i ponownie kliknij **Uruchom.cmd**.

Jeżeli strona się nie otwiera, sprawdź, czy okno BeFit nadal jest otwarte i czy wpisany adres to dokładnie **http://localhost:5080**. Jeśli w oknie pojawi się błąd, zrób zdjęcie komunikatu i pokaż je osobie pomagającej w uruchomieniu.

## 3. Załóż konto

1. Na stronie BeFit kliknij **Rejestracja**.
2. Wpisz swój adres e-mail.
3. Wymyśl hasło mające od **10 do 128 znaków** i wpisz je w obu polach hasła.
4. Kliknij **Utwórz konto**.

Przy kolejnym wejściu wybierz **Logowanie** i podaj ten sam e-mail oraz hasło.

## 4. Dodaj trening

1. Kliknij **Moje treningi**.
2. Kliknij **Dodaj trening**.
3. Wpisz nazwę, np. „Trening poniedziałkowy”.
4. Wybierz datę i godzinę rozpoczęcia oraz zakończenia. Zakończenie musi być później niż rozpoczęcie.
5. Jeśli chcesz, dopisz notatkę.
6. Kliknij **Zapisz**.

## 5. Zapisz wykonane ćwiczenia

1. Otwórz swój trening, klikając jego nazwę na liście **Moje treningi**.
2. Kliknij **Dodaj wykonanie**.
3. Wybierz trening i ćwiczenie z list.
4. Wpisz obciążenie w kilogramach, liczbę serii i liczbę powtórzeń w jednej serii.
5. Kliknij **Zapisz**.
6. Powtórz te kroki dla kolejnych ćwiczeń.

Przykład: obciążenie **12,5**, serie **3**, powtórzenia **10** oznaczają trzy serie po dziesięć powtórzeń z ciężarem 12,5 kg. Jeśli ćwiczysz bez dodatkowego ciężaru, wpisz **0**.

## 6. Popraw wpis lub sprawdź wyniki

- Aby poprawić dane, kliknij **Edytuj** przy wybranym wpisie, zmień dane i kliknij **Zapisz**.
- Aby usunąć wpis, kliknij **Usuń** i potwierdź. Usunięcie treningu usuwa też zapisane w nim ćwiczenia.
- Kliknij **Statystyki**, aby zobaczyć swoje wyniki z ostatnich czterech tygodni.
- Lista **Typy ćwiczeń** jest dostępna także bez logowania. Każdy użytkownik widzi tylko własne treningi i wyniki.

## 7. Zakończ pracę i wróć później

1. Kliknij **Wyloguj**.
2. Zamknij kartę BeFit w przeglądarce.
3. Zamknij okno uruchomione przez **Uruchom.cmd**.

Aby wrócić, ponownie otwórz **Uruchom.cmd**, poczekaj na uruchomienie i wejdź na **http://localhost:5080**.

Treningi zapisują się automatycznie po kliknięciu **Zapisz**. Nie usuwaj folderu aplikacji ani znajdującego się w nim folderu **App_Data** — tam są przechowywane konta i treningi.

## Dodatkowo: konto administratora

Ten krok jest potrzebny tylko osobie, która ma dodawać, zmieniać i usuwać pozycje na liście **Typy ćwiczeń**. Do zwykłego zapisywania treningów nie jest potrzebny.

1. Najpierw załóż konto według punktu 3.
2. Zamknij okno uruchomione przez **Uruchom.cmd**.
3. Otwórz folder BeFit, w którym jest plik **Uruchom.cmd**.
4. Kliknij pasek u góry okna folderu, w którym wyświetla się jego położenie. Wpisz **cmd** i naciśnij **Enter**.
5. W otwartym oknie wklej poniższą linię. Zamiast `adres@example.pl` wpisz e-mail konta, które przed chwilą utworzyłeś:

   ```text
   dotnet run --no-launch-profile -- --make-admin adres@example.pl
   ```

6. Naciśnij **Enter** i poczekaj na komunikat o nadaniu roli administratora.
7. Zamknij to okno. Uruchom ponownie **Uruchom.cmd** i zaloguj się do BeFit. Jeśli jesteś już zalogowany, najpierw się wyloguj.

Po zalogowaniu na to konto pojawią się opcje zarządzania typami ćwiczeń.
