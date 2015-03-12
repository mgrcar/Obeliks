O projektu Obeliks
==================

Dolgoročni cilj projekta Obeliks je izdelava in nadgrajevanje najbolj natančnega statističnega označevalnika za slovenski jezik. Oblikoskladenjsko označevanje je proces pripisovanja oblikoslovnih (in deloma skladenjskih) lastnosti besedam v poljubnem besedilu. Tako označeno besedilo je predpogoj za delovanje večine aplikacij, ki temeljijo na analizi naravnega jezika. Označevanje slovenskih besedil je zelo težak problem, saj mora algoritem za označevanje pravilno izbirati med skoraj dva tisoč oznakami  (število različnih oznak za označevanje angleškega besedila je zgolj okoli šestdeset).

*The aim of the Obeliks project is to develop the most accurate statistical tagger for the Slovene language. Morphosyntactic tagging is the process of categorizing a word in a text into a particular part of speech category and describing it with various morphological features related to that category. This kind of markup is required by many applications involving natural language processing. The tagging of Slovene texts represents a major difficulty as the algorithm needs to choose from nearly 2,000 possible tags (as opposed to English where the number of tags is around 60).*

O tej datoteki
==============

V tej datoteki se nahajajo navodila za uporabo oblikoslovnega označevalnika Obeliks iz ukazne vrstice. Označevalnik Obeliks je razdeljen na tri programe: LemmatizerTrain (program za izgradnjo lematizacijskega modela), PosTaggerTrain (program za izgradnjo modela za označevanje) in PosTaggerTag (program za označevanje besedil). Ti programi so bolj podrobno opisani v naslednjih razdelkih, kjer podajamo tudi primere uporabe in hitre povezave do datotek, ki so potrebne za delovanje programov in podanih primerov uporabe.

Program LemmatizerTrain
-----------------------

Program LemmatizerTrain iz označenega besedila v formatu XML-TEI zgradi model za lematizacijo besed (tj. za prevedbo besed v njihove osnovne oblike) in ga shrani v izhodno datoteko.

Navodila za uporabo programa LemmatizerTrain:

```text
LemmatizerTrain [<nastavitve>] <korpus_xml> <model_bin>

<nastavitve>:  Glej spodaj.
<korpus_xml>:  Ucni korpus v formatu XML-TEI (vhod).
<model_bin>:   Model za lematizacijo (izhod).

Nastavitve:
-v              Izpisovanje na zaslon (verbose).
                (privzeto: ni izpisovanja)
-t              Upostevanje oblikoslovnih oznak.
                (privzeto: oblikoslovne oznake niso upostevane)
-o              Optimizacija lematizacijskega drevesa (oznake SSJ).
                (privzeto: optimizacija se ne izvede)
-l:ime_datoteke Ucenje iz podanega leksikona.
                (privzeto: ucenje brez leksikona)
```

Primer uporabe:

```text
LemmatizerTrain -v -t -o -l:SloveneLexicon.txt TrainingCorpus500k.xml LemmatizerModel.bin
```

Izpis na zaslon:

```text
Nalagam ucni korpus ...
586244 / 586244
Nalagam leksikon ...
2786365
Gradim model za lematizacijo ...
Optimiram lematizacijsko drevo ...
Zapisujem model ...
Koncano.
```

### Datoteke za prenos ###

* [LemmatizerTrain.exe in pripadajoče datoteke](http://sourceforge.net/projects/obeliks/files/ObeliksReleases/ObeliksMay2012.zip/download) (program)
* [TrainingCorpus500k.xml](http://sourceforge.net/projects/obeliks/files/Resources/TrainingCorpus500k.xml.zip/download) (vhodna datoteka; učni korpus)
* [SloveneLexicon.txt](http://sourceforge.net/projects/obeliks/files/Resources/SloveneLexicon.txt.zip/download) (vhodna datoteka; dodatni učni podatki)
* [LemmatizerModel.bin](http://sourceforge.net/projects/obeliks/files/Resources/LemmatizerModel.bin.zip/download) (izhodna datoteka; model za lematizacijo)

Program PosTaggerTrain
----------------------

Program PosTaggerTrain iz označenega besedila v formatu XML-TEI zgradi model za označevanje besedila in ga shrani v izhodno datoteko.

Navodila za uporabo programa PosTaggerTrain:

```text
PosTaggerTrain [<nastavitve>] <korpus_xml> <model_bin>

<nastavitve>: Glej spodaj.
<korpus_xml>: Ucni korpus v formatu XML-TEI (vhod).
<model_bin>:  Model za oznacevanje (izhod).

Nastavitve:
-v              Izpisovanje na zaslon (verbose).
                (privzeto: ni izpisovanja)
-c:<int>=0>     Parameter za izgradnjo modela (cut-off).
                (privzeto: 2)
-i:<int>0>      Stevilo iteracij za izgradnjo modela.
                (privzeto: 50)
-t:<int>0>      Stevilo niti za paralelizacijo algoritma.
                (privzeto: 1)
-l:ime_datoteke Uporaba leksikona.
                (privzeto: oznacevanje brez leksikona)
```

Primer uporabe:

```text
PosTaggerTrain -v -t:4 -l:SloveneLexicon.txt TrainingCorpus500k.xml TaggerModel.bin
```

Izpis na zaslon:

```text
Nalagam ucni korpus ...
Nalagam leksikon ...
Poraba pomnilnika (drevo koncnic): 240.92 MB
Poraba pomnilnika (propagirane oznake): 34.09 MB
Pripravljam vektorje znacilk ...
586244 / 586244
Gradim model ...
Creating observation matrix ...
586244 / 586244
Performing cut-off ...
Preparing structures ...
Entering main loop ...
Iteration 1 / 50 ...
Updating expectations ...
Initiating 4 threads ...
Pass 1: 1104 / 1104
Pass 2: 1104 / 1104
Updating lambdas ...
...
Iteration 50 / 50 ...
Updating expectations ...
Initiating 4 threads ...
Pass 1: 1104 / 1104
Pass 2: 1104 / 1104
Updating lambdas ...
Trajanje gradnje modela: 11:09:53.538.
Zapisujem model ...
Koncano.
```

### Datoteke za prenos ###

* [PosTaggerTrain.exe in pripadajoče datoteke](http://sourceforge.net/projects/obeliks/files/ObeliksReleases/ObeliksMay2012.zip/download) (program)
* [TrainingCorpus500k.xml](http://sourceforge.net/projects/obeliks/files/Resources/TrainingCorpus500k.xml.zip/download) (vhodna datoteka; učni korpus)
* [SloveneLexicon.txt](http://sourceforge.net/projects/obeliks/files/Resources/SloveneLexicon.txt.zip/download) (vhodna datoteka; dodatni učni podatki)
* [TaggerModel.bin](http://sourceforge.net/projects/obeliks/files/Resources/TaggerModel.bin.zip/download) (izhodna datoteka; model za označevanje)

Program PosTaggerTag
--------------------

Program PosTaggerTag omogoča oblikoslovno označevanje besedila. Za svoje delovanje potrebuje vhodno besedilo bodisi v tekstovnem formatu bodisi v formatu XML-TEI in model za označevanje, zgrajen s programom PosTaggerTrain. Na podlagi teh datotek PosTaggerTag tvori datoteko z označenim besedilom v formatu XML-TEI. Možno je podati tudi datoteko z modelom za lematizacijo, zgrajenim s programom LemmatizerTrain. V tem primeru izhodna datoteka vsebuje tudi besede v osnovnih oblikah.

Navodila za uporabo programa PosTaggerTag:

```text
PosTaggerTag [<nastavitve>] <vhodne_datoteke> <model_bin> <oznaceni_korpus_xml>

<nastavitve>:     Glej spodaj.
<besedilo>:       Besedilo za oznacevanje (vhod).
<model_bin>:      Model za oznacevanje (vhod).
<oznaceni_korpus_xml>:
                  Oznaceni korpus v formatu XML-TEI (izhod).

Nastavitve:
-v                Izpisovanje na zaslon (verbose).
                  (privzeto: ni izpisovanja)
-lem:ime_datoteke Model za lematizacijo.
                  (privzeto: lematizacija se ne izvede)
-s                Vkljuci podmape pri iskanju vhodnih besedil.
                  (privzeto: isci samo v podani mapi)
-t                Uporaba razclenjevalnika SSJ.
                  (privzeto: ne uporabi razclenjevalnika SSJ)
-o                Prepisi obstojece izhodne datoteke.
                  (privzeto: ne prepisi obstojecih datotek)
```

Prvi primer uporabe:

```text
PosTaggerTag -lem:ssj500kv1_0-fold-01-train_2012_lem.bin -v -o ssj500kv1_0-fold-01-validate_2012.xml ssj500kv1_0-fold-01-train_2012.bin ssj500kv1_0-fold-01-validate_2012_tagged.xml
```

Izpis na zaslon:

```text
Nalagam model za oznacevanje ...
Nalagam model za lematizacijo ...
Mapa z vhodnimi datotekami: .
Iskalni vzorec: ssj500kv1_0-fold-01-validate_2012.xml
Nalagam C:\Users\Administrator\Desktop\Obeliks\ssj500kv1_0-fold-01-validate_2012.xml ...
Oznacujem besedilo ...
58552 / 58552
Trajanje oznacevanja: 00:11:56.990.
Zapisujem oznaceno besedilo v datoteko ssj500kv1_0-fold-01-validate_2012_tagged.xml ...
Koncano.
Tocnost na znanih besedah: ................... 93.25% (53675 / 57563)
Tocnost na neznanih besedah: ................. 53.69% (531 / 989)
Skupna tocnost: .............................. 92.58% (54206 / 58552)
Tocnost na znanih besedah (POS): ............. 98.64% (56780 / 57563)
Tocnost na neznanih besedah (POS): ........... 80.49% (796 / 989)
Skupna tocnost (POS): ........................ 98.33% (57576 / 58552)
Tocnost na znanih besedah (brez locil): ...... 92.25% (44601 / 48348)
Tocnost na neznanih besedah (brez locil): .... 53.46% (526 / 984)
Skupna tocnost (brez locil): ................. 91.48% (45127 / 49332)
Tocnost na znanih besedah (POS, brez locil):   98.38% (47565 / 48348)
Tocnost na neznanih besedah (POS, brez locil): 80.39% (791 / 984)
Skupna tocnost (POS, brez locil): ............ 98.02% (48356 / 49332)
Tocnost lematizacije (brez locil): ........... 97.86% (48275 / 49332)
Tocnost lematizacije (male crke, brez locil):  98.60% (48642 / 49332)
Tocnost detekcije konca stavka: .............. 95.19% (2792 / 2933)
```

Drugi primer uporabe:

```text
PosTaggerTag -lem:LemmatizerModel.bin -v -o -t ClanekDelo11maj2012.txt TaggerModel.bin ClanekDelo11maj2012.xml
```

Izpis na zaslon:

```text
Nalagam model za oznacevanje ...
Nalagam model za lematizacijo ...
Mapa z vhodnimi datotekami: .
Iskalni vzorec: ClanekDelo11maj2012.txt
Nalagam C:\Users\Administrator\Desktop\Obeliks\ClanekDelo11maj2012.txt ...
Oznacujem besedilo ...
691 / 691
Trajanje oznacevanja: 00:00:06.029.
Zapisujem oznaceno besedilo v datoteko ClanekDelo11maj2012.xml ...
Koncano.
```

### Datoteke za prenos ###

* [PosTaggerTag.exe in pripadajoče datoteke](http://sourceforge.net/projects/obeliks/files/ObeliksReleases/ObeliksMay2012.zip/download) (program)
* [ssj500kv1_0-fold-01-train_2012_lem.bin, ssj500kv1_0-fold-01-train_2012.bin, ssj500kv1_0-fold-01-validate_2012.xml, ssj500kv1_0-fold-01-validate_2012_tagged.xml](http://sourceforge.net/projects/obeliks/files/Resources/ValidationResourcesFold1.zip/download) (model za lematizacijo, model za označevanje, vhodna in izhodna datoteka; prvi primer uporabe)
* [LemmatizerModel.bin](http://sourceforge.net/projects/obeliks/files/Resources/LemmatizerModel.bin.zip/download) (vhodna datoteka; model za lematizacijo; drugi primer uporabe)
* [TaggerModel.bin](http://sourceforge.net/projects/obeliks/files/Resources/TaggerModel.bin.zip/download) (vhodna datoteka; model za označevanje; drugi primer uporabe)
* [ClanekDelo11maj2012.txt, ClanekDelo11maj2012.xml](http://sourceforge.net/projects/obeliks/files/Resources/ClanekDelo11maj2012.zip/download) (vhodna in izhodna datoteka; drugi primer uporabe)

Opomba
------

Vsi opisani programi so izdelani v razvojnem okolju Microsoft Visual Studio 2008. Za svoje delovanje potrebujejo zagonsko okolje .NET Framework 3.5. Če slednjega še nimate nastanjenega na računalniku, si ga prenesite s spleta ([povezava](http://www.microsoft.com/downloads/details.aspx?FamilyID=AB99342F-5D1A-413D-8319-81DA479AB0D7&displaylang=en)), zaženite nastanitveni program (tj. dotnetfx35setup.exe) in sledite navodilom za nastanitev.

Zadnja inačica in izvorna koda
------------------------------

Zadnja (delovna) inačica izvorne kode je na voljo v naših GIT-repozitorijih:

* Označevalnik se nahaja v repozitoriju https://github.com/mgrcar/Obeliks.git
* LATINO, knjižnica, ki je potrebna za delovanje označevalnika, se nahaja v repozitoriju https://github.com/SowaLabs/LATINO.git

Zagonske datoteke za operacijski sistem Windows dobite [tukaj](http://sourceforge.net/projects/obeliks/files/ObeliksReleases/ObeliksMar2013.zip/download).

Modeli, ki so združljivi z zadnjo inačico označevalnika, se nahajajo [tukaj](http://sourceforge.net/projects/obeliks/files/Resources/ModelsMar2013.zip/download).

Zasluge
-------

Oblikoslovni označevalnik, dostopen na tej spletni strani, so v programskem jeziku C# implementirali Miha Grčar, Matjaž Juršič in Jan Rupnik pod vsebinskim vodstvom Simona Kreka. Segmentacijska, tokenizacijska in lematizacijska pravila, vključena v označevalnik, so  izdelali Simon Krek, Kaja Dobrovoljc in Miha Grčar.

Lematizator, uporabljen v označevalniku, je zasnoval Matjaž Juršič v okviru svojega diplomskega dela pod mentorstvom Igorja Mozetiča. Več informacij o lematizatorju lahko dobite na spletni strani http://lemmatise.ijs.si/Software.

Učni korpusi, uporabljeni pri učenju označevalnika in lematizatorja, so bili izdelani v okviru projektov Jezikoslovno označevanje slovenskega jezika (JOS) in Sporazumevanje v slovenskem jeziku (SSJ). Informacije o projektu JOS in pripadajočih korpusih so na spletni strani http://nl.ijs.si/jos. Informacije o učnem korpusu, izdelanem v okviru projekta SSJ, so na spletni strani [http://www.slovenščina.eu/Vsebine/Sl/Kazalniki/K10.aspx](http://www.slovenščina.eu/Vsebine/Sl/Kazalniki/K10.aspx).

Leksikon, uporabljen pri učenju označevalnika in lematizatorja, je bil izdelan v okviru projekta Sporazumevanje v slovenskem jeziku. Več informacij o leksikonu lahko dobite na spletni strani [http://www.slovenščina.eu/Vsebine/Sl/Kazalniki/K12.aspx](http://www.slovenščina.eu/Vsebine/Sl/Kazalniki/K12.aspx).

Oblikoslovni označevalnik je bil izdelan v okviru projekta Sporazumevanje v slovenskem jeziku. Več informacij o projektu lahko dobite na spletni strani [http://www.slovenščina.eu](http://www.slovenščina.eu).

Operacijo delno financira Evropska unija iz [Evropskega socialnega sklada](http://euskladi.si) ter [Ministrstvo za izobraževanje, znanost, kulturo in šport](http://mss.gov.si). Operacija se izvaja v okviru Operativnega programa razvoja človeških virov za obdobje 2007&#8211;2013, razvojne prioritete: razvoj človeških virov in vseživljenjskega učenja; prednostne usmeritve: izboljšanje kakovosti in učinkovitosti sistemov izobraževanja in usposabljanja 2007&#8211;2013.

Reference
---------

* Giménez, J., Màrquez, L. (2004): SVMTool: A General POS Tagger Generator Based on Support Vector Machines. Proceedings of the Fourth International Conference on Language Resources and Evaluation (LREC’04), Lisbon. ([pdf](http://www.lsi.upc.es/~nlp/SVMTool/lrec2004-gm.pdf))
* Juršič, M., Mozetič, I., Lavrač, N. (2007): Learning Ripple Down Rules for Efficient Lemmatization. Proceedings of the 10th International Multiconference Information Society, IS 2007, str. 206&#8211;209, Ljubljana. ([pdf](http://kt.ijs.si/software/LemmaGen/v2/doc/LemmaGen.pdf))
* Nigam, K., Lafferty, J., McCallum, A. (1999): Using Maximum Entropy for Text Classification. Proceedings of IJCAI-99 Workshop on Machine Learning for Information Filtering, str. 61&#8211;67.
* Erjavec, T., Krek, S. (2008): Oblikoskladenjske specifikacije in označeni korpusi JOS. Zbornik Šeste konference Jezikovne tehnologije, Ljubljana. ([pdf](http://nl.ijs.si/jos/bib/jos_isltc08.pdf))
* Erjavec, T., Krek, S. (2008): The JOS Morphosyntactically Tagged Corpus of Slovene. Proceedings of the Sixth International Conference on Language Resources and Evaluation (LREC’08), Marrakech. ([pdf](http://nl.ijs.si/jos/bib/jos_lrec08.pdf))

Drugo gradivo
-------------

* [Miha Grčar: Oblikoskladenjski označevalnik SSJ](http://videolectures.net/korpusi2010_grcar_oos), predstavitev na konferenci [Korpusi, več kot le statistika](http://www.slovenščina.eu/vsebine/Sl/Dogodki/Korpusi/Program.aspx) (Fakulteta za družbene vede, Ljubljana, 5. februar 2010)