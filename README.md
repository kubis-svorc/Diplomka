# Diplomová práca
Autor: Jakub Švorc



Konakt: svorc7@uniba.sk



Vedúci práce: RNDr. Ľudmila Jašková, PhD.

Názov:



Edukačné prostredie na programovanie hudby prístupné pre nevidiacich žiakov sekundárneho vzdelávania
---


Anotácia
---
Autor vytvorí programovacie prostredie s vlastným kompilátorom alebo
interpreterom. Základné príkazy zabudovaného programovacieho jazyka budú
slúžiť na prehratie tónov zvoleným hudobným nástrojom. Okrem toho bude
možné použiť aj komplikovanejšie štruktúry, ako je cyklus, príkaz vetvenia,
podprogram, vlákno.
Editor kódu bude mať zabudovanú kontrolu syntaxe a funkciu prediktívnej
ponuky príkazov.
Prostredie bude prístupné pre čítač obrazovky a bude plne ovládateľné pomocou
klávesnice. Nevidiacim používateľom umožní okrem bežnej práce s textom aj
jednoduchým spôsobom získať prehľad o štruktúre vytvoreného kódu.
Použiteľnosť výslednej aplikácie pre cieľového používateľa bude zabezpečená
vďaka výskumu vývojom (design based research), t.j. iteratívnym vývojom
a overovaním s rôznymi typmi používateľov (nevidiaci programátor, učiteľ
nevidiacich žiakov, nevidiaci žiak).


Cieľ
---
Vytvoriť programovacie prostredie umožňujúce programovať hudbu
pozostávajúcu z viacerých paralelne znejúcich melódií. Dôraz bude kladený
na zabezpečenie plnej prístupnosti a efektívnej práce s editorom kódu pre žiakov
so zrakovým postihnutím.


Denník
---
Projektový seminár (1) - Letný semester:


**13.02.2023 - 19.02.2023 :**

* Prehľad možných nástrojov - jazyk, knižnice

* Spracovanie literatúry

* Konzultácia so školiteľkou


**20.02.2023 - 26.02.2023 :**

* Vytvorenie demo projektu v JavaFX a Python, testovanie kompatibility a dostupnosti UI elementov voči NVDA 

**27.02.2023 - 05.03.2023 :**

* Zmena technológie - .NET WPF

* Návrh základného UI

* Oboznámenie sa so syntaxov nami navrhnutého jazyka


**06.03.2023 - 12.03.2023 :**

* Jednoduchý interpreter na priame vykonávanie jednoduchých príkazov (tón) - testovanie knižníc.

* Nepodporuje prácu s premennými, podprogrami, vetvenia a podmienené cykly.

* Konzultácia so školiteľou, ukážka interpretera a chodu aplikácie

**13.03.2023 - 19.03.2023 :**

* Začiatok prerábky interpretera na kompilátor

* Úprava UI - odstránenie nepotrebných prvkov


**20.03.2023 - 26.03.2023 :**

* Začiatok implementácie virtuálnej mašiny.

* Test vykonávania jednoduchých základných príkazov - tón, nástroj.

* Zavedenie enum typov pre tóny a nástroje.


**27.03.2023 - 02.04.2023 :**

* Úprava kompilátora - vytváranie syntaktického stromu namiesto priameho vkladania inštrukcií - problém s cyklom.

* Vytvorenie tried a hierarchie

* Implementácia syntaktickej kontroly.

* Konzultácia so školiteľkou


**03.04.2023 - 09.04.2023 :**

* Úprava kompilátora na spracovanie dodatočných parametrov pri vykonávaní základného príkazu tón, doplnenie inštrukcií.

* Úprava virtuálnej mašiny na spracovanie a vyhodnotenie týchto parametrov - z mem.

* Doplnenie terminálu na chybové hlásenia a výpis.

* Úprava GUI podľa pripomienok školiteľky.

**10.04.2023 - 16.04.2023 :**

* Doplnenie samostatenj inštrukcie do kompilátora a virtuálnej mašiny na nastavenia nástroja, samostatný príkaz.

* Implementovanie for cyklu s konštantným počtom opakovaní.


**17.04.2023 - 23.04.2023 :**

* Implementácia príkazov na zmenu dĺžky tónu, zmenu hlasitosti a zmenu hlasitosti v jednotlivých výstupoch.

* Implementácia vyhodnocovania výrazov.

* Implementovanie práce s premennými.


**24.03.2023 - 30.04.2023 :**

* Predošlé riešenie nefunguje, chýbajúca funkcionalita :

zmenu hlasitosti v jednotlivých výstupoch,

práca s premennými - nedajú sa používať, iba deklarovať,

výrazy sa dajú vyhodnocovať, nadväzujú však na problém s premennými.

* Testovanie doterajšej funkčnosti s čitačom obrazovky NVDA.


**01.05.2023 - 07.05.2023 :**

* Príprava na prezentáciu - LateX, GitHub repo, prezentácia, upratanie kódu, súborov

* Konzultácia so školiteľkou


**10.07.2023 - 16.07.2023 :**

* For-cyklus pracuje s premennymi a celková práca s nimi, pripravane triedy na nekonecny cyklus

