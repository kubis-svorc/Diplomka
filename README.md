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

**24.07.2023 - 30.07.2023**

* Implementacia nekonecneho cyklus, pripravene triedy na logicke operacie (<, >, =, !=, <=, >=, +, /, -, *)


**31.07.2023 - 06.08.2023**

* Implementacia cyklu a logickych operacii do VM


**07.08.2023 - 13.08.2023**

* Implementacia primitivnych podprogramov (bez parametrov, bez navratovych hodnot), bugfix niektorych implementacii funkcie Generate())


**14.08.2023 - 20.08.2023

* Implementacia vetvenia (if else), aj s premennými (nefunguje)

*todo: premenne vo vetveniach

* Bugfix


**21.08.2023 - 27.08.2023

* Implementácia vlákien na paralelné prehrávanie (nefunguje zatiaľ správne)

*todo: premenne vo vetveniach


**28.08.2023 - 03.09.2023

* Iný prípstup k implementácia vlákien a paralelnéhp prehrávanie (tiež nefunguje)

*todo: premenne vo vetveniach


**04.09.2023 - 10.09.2023

* Zmena logiky prehrávania zvukov -> vlákna (iba hrubý náčrt, ešte treba vyskúšať viac ako 1)

* Nekonecne cykly - problem (?)

*todo: premenne vo vetveniach


**11.09.2023 - 17.09.2023

* Implementácia vlákien s novým spôsobom prehrávania (zoznamy)

* Nekonecny cyklus nemusi byt - odstranene

* Bugfix

*todo: premenne vo vetveniach


**18.09.2023 - 24.09.2023

* System napoved a navigacia na neho 


**25.09.2023 - 01.10.2023
 
* Spracovanie clankov o sposobe vyvoja softveru


**02.10.2023 - 08.10.2023

* Implementacia systemu napoved (nefunguje spravne)

**09.10.2023 - 15.10.2023

* Oprava systemu napoved (pokracovanie) a niektorych dodatocnych skratiek


**16.10.2023 - 22.10.2023

* Zavedenie klavesovych skratiek a premapovanie standardnych windowsovych skratiek do aplikacie


**23.10.2023 - 29.10.2023

* Implementacia navigacie po kode (CTRL + H)


**30.10.2023 - 05.11.2023

* Zavedenie async metod na udrzovanie okna reaktivnom stave

* todo: odstranit bug - pocas prepisovanie textu spadne program


**06.10.2023 - 12.11.2023

* Zavedenie dalsich metod ako async, zavedenie systemu na zastavenie prehravania ked program bezi dlho alebo sa zasekne (CTRL+SHIFT+F5)

* BUGFIX : niektore async metody zhadzovali aplikaciu

**13.10.2023 - 19.11.2023

* Implementacia saveovania programu do MIDI vystupu (nefunguje)

* Prelozenie niektorych textov do slovenciny

* Pisanie DP 

**20.10.2023 - 26.11.2023

* Implementacia savovanie programu do MIDI vystupu (iny sposob, funguje(?))

* Pisanie DP

**27.10.2023 - 03.12.2023

* Priprava prezentacie

*todo: premenne vo vetveniach



