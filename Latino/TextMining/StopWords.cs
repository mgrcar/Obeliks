/*==========================================================================;
 *
 *  (c) 2008-09 JSI.  All rights reserved.
 *
 *  File:          StopWords.cs
 *  Version:       1.0
 *  Desc:		   Stop word lists
 *  Author:        Miha Grcar
 *  Created on:    Dec-2008
 *  Last modified: Jan-2009
 *  Revision:      Jan-2009
 *
 ***************************************************************************/

namespace Latino.TextMining
{
    /* .-----------------------------------------------------------------------
       |
       |  Static class StopWords
       |
       '-----------------------------------------------------------------------
    */
    public static class StopWords
    {
        // this list is taken from http://snowball.tartarus.org/algorithms/english/stop.txt
        public static Set<string>.ReadOnly EnglishStopWords 
            = new Set<string>.ReadOnly(new Set<string>(new string[] {
                              // PRONOUNS FORMS
                                // 1st person singular
                "i",              // subject, always in upper case of course
                "me",             // object
                "my",             // possessive adjective
                                  // the possessive pronoun `mine' is best suppressed, because of the
                                  // sense of coal-mine etc.
                "myself",         // reflexive
                                // 1st person plural
                "we",             // subject
                // us             // object
                                  // care is required here because US = United States. It is usually
                                  // safe to remove it if it is in lower case.
                "our",            // possessive adjective
                "ours",           // possessive pronoun
                "ourselves",      // reflexive
                                // second person (archaic `thou' forms not included)
                "you",            // subject and object
                "your",           // possessive adjective
                "yours",          // possessive pronoun
                "yourself",       // reflexive (singular)
                "yourselves",     // reflexive (plural)
                                // third person singular
                "he",             // subject
                "him",            // object
                "his",            // possessive adjective and pronoun
                "himself",        // reflexive
                "she",            // subject
                "her",            // object and possessive adjective
                "hers",           // possessive pronoun
                "herself",        // reflexive
                "it",             // subject and object
                "its",            // possessive adjective
                "itself",         // reflexive
                                // third person plural
                "they",           // subject
                "them",           // object
                "their",          // possessive adjective
                "theirs",         // possessive pronoun
                "themselves",     // reflexive
                                // other forms (demonstratives, interrogatives)
                "what",
                "which",
                "who",
                "whom",
                "this",
                "that",
                "these",
                "those",
                              // VERB FORMS (using F.R. Palmer's nomenclature)
                                // BE
                "am",             // 1st person, present
                "is",             // -s form (3rd person, present)
                "are",            // present
                "was",            // 1st person, past
                "were",           // past
                "be",             // infinitive
                "been",           // past participle
                "being",          // -ing form
                                // HAVE
                "have",           // simple
                "has",            // -s form
                "had",            // past
                "having",         // -ing form
                                // DO
                "do",             // simple
                "does",           // -s form
                "did",            // past
                "doing",          // -ing form
                 // The forms below are, I believe, best omitted, because of the significant
                 // homonym forms:
                 //  He made a WILL
                 //  old tin CAN
                 //  merry month of MAY
                 //  a smell of MUST
                 //  fight the good fight with all thy MIGHT
                 // would, could, should, ought might however be included
                 //           // AUXILIARIES
                 //             // WILL
                 //will
                "would",
                 //             // SHALL
                 //shall
                "should",
                 //             // CAN
                 //can
                "could",
                 //             // MAY
                 //may
                 //might
                 //             // MUST
                 //must
                 //             // OUGHT
                "ought",
                              // COMPOUND FORMS, increasingly encountered nowadays in 'formal' writing
                                // pronoun + verb
                "i'm",
                "you're",
                "he's",
                "she's",
                "it's",
                "we're",
                "they're",
                "i've",
                "you've",
                "we've",
                "they've",
                "i'd",
                "you'd",
                "he'd",
                "she'd",
                "we'd",
                "they'd",
                "i'll",
                "you'll",
                "he'll",
                "she'll",
                "we'll",
                "they'll",
                                // verb + negation
                "isn't",
                "aren't",
                "wasn't",
                "weren't",
                "hasn't",
                "haven't",
                "hadn't",
                "doesn't",
                "don't",
                "didn't",
                                // auxiliary + negation
                "won't",
                "wouldn't",
                "shan't",
                "shouldn't",
                "can't",
                "cannot",
                "couldn't",
                "mustn't",
                                // miscellaneous forms
                "let's",
                "that's",
                "who's",
                "what's",
                "here's",
                "there's",
                "when's",
                "where's",
                "why's",
                "how's",
                                // rarer forms
                 // daren't needn't
                                // doubtful forms
                 // oughtn't mightn't
                              // ARTICLES
                "a",
                "an",
                "the",
                              // THE REST (Overlap among prepositions, conjunctions, adverbs etc is so
                              // high, that classification is pointless.)
                "and",
                "but",
                "if",
                "or",
                "because",
                "as",
                "until",
                "while",
                "of",
                "at",
                "by",
                "for",
                "with",
                "about",
                "against",
                "between",
                "into",
                "through",
                "during",
                "before",
                "after",
                "above",
                "below",
                "to",
                "from",
                "up",
                "down",
                "in",
                "out",
                "on",
                "off",
                "over",
                "under",
                "again",
                "further",
                "then",
                "once",
                "here",
                "there",
                "when",
                "where",
                "why",
                "how",
                "all",
                "any",
                "both",
                "each",
                "few",
                "more",
                "most",
                "other",
                "some",
                "such",
                "no",
                "nor",
                "not",
                "only",
                "own",
                "same",
                "so",
                "than",
                "too",
                "very"}));

        // this list is taken from http://snowball.tartarus.org/algorithms/french/stop.txt
        public static Set<string>.ReadOnly FrenchStopWords 
            = new Set<string>.ReadOnly(new Set<string>(new string[] {
                "au",             //  a + le
                "aux",            //  a + les
                "avec",           //  with
                "ce",             //  this
                "ces",            //  these
                "dans",           //  with
                "de",             //  of
                "des",            //  de + les
                "du",             //  de + le
                "elle",           //  she
                "en",             //  `of them' etc
                "et",             //  and
                "eux",            //  them
                "il",             //  he
                "je",             //  I
                "la",             //  the
                "le",             //  the
                "leur",           //  their
                "lui",            //  him
                "ma",             //  my (fem)
                "mais",           //  but
                "me",             //  me
                "même",           //  same; as in moi-même (myself) etc
                "mes",            //  me (pl)
                "moi",            //  me
                "mon",            //  my (masc)
                "ne",             //  not
                "nos",            //  our (pl)
                "notre",          //  our
                "nous",           //  we
                "on",             //  one
                "ou",             //  where
                "par",            //  by
                "pas",            //  not
                "pour",           //  for
                "qu",             //  que before vowel
                "que",            //  that
                "qui",            //  who
                "sa",             //  his, her (fem)
                "se",             //  oneself
                "ses",            //  his (pl)
                "son",            //  his, her (masc)
                "sur",            //  on
                "ta",             //  thy (fem)
                "te",             //  thee
                "tes",            //  thy (pl)
                "toi",            //  thee
                "ton",            //  thy (masc)
                "tu",             //  thou
                "un",             //  a
                "une",            //  a
                "vos",            //  your (pl)
                "votre",          //  your
                "vous",           //  you
                               //  single letter forms
                "c",              //  c'
                "d",              //  d'
                "j",              //  j'
                "l",              //  l'
                "à",              //  to, at
                "m",              //  m'
                "n",              //  n'
                "s",              //  s'
                "t",              //  t'
                "y",              //  there
                               // forms of être (not including the infinitive):
                "été",
                "étée",
                "étées",
                "étés",
                "étant",
                "étante",
                "étants",
                "étantes",
                "suis",
                "es",
                "est",
                "sommes",
                "êtes",
                "sont",
                "serai",
                "seras",
                "sera",
                "serons",
                "serez",
                "seront",
                "serais",
                "serait",
                "serions",
                "seriez",
                "seraient",
                "étais",
                "était",
                "étions",
                "étiez",
                "étaient",
                "fus",
                "fut",
                "fûmes",
                "fûtes",
                "furent",
                "sois",
                "soit",
                "soyons",
                "soyez",
                "soient",
                "fusse",
                "fusses",
                "fût",
                "fussions",
                "fussiez",
                "fussent",
                               // forms of avoir (not including the infinitive):
                "ayant",
                "ayante",
                "ayantes",
                "ayants",
                "eu",
                "eue",
                "eues",
                "eus",
                "ai",
                "as",
                "avons",
                "avez",
                "ont",
                "aurai",
                "auras",
                "aura",
                "aurons",
                "aurez",
                "auront",
                "aurais",
                "aurait",
                "aurions",
                "auriez",
                "auraient",
                "avais",
                "avait",
                "avions",
                "aviez",
                "avaient",
                "eut",
                "eûmes",
                "eûtes",
                "eurent",
                "aie",
                "aies",
                "ait",
                "ayons",
                "ayez",
                "aient",
                "eusse",
                "eusses",
                "eût",
                "eussions",
                "eussiez",
                "eussent"}));

        // this list is taken from http://snowball.tartarus.org/algorithms/spanish/stop.txt
        public static Set<string>.ReadOnly SpanishStopWords
            = new Set<string>.ReadOnly(new Set<string>(new string[] {
                "de",             //  from, of
                "la",             //  the, her
                "que",            //  who, that
                "el",             //  the
                "en",             //  in
                "y",              //  and
                "a",              //  to
                "los",            //  the, them
                "del",            //  de + el
                "se",             //  himself, from him etc
                "las",            //  the, them
                "por",            //  for, by, etc
                "un",             //  a
                "para",           //  for
                "con",            //  with
                "no",             //  no
                "una",            //  a
                "su",             //  his, her
                "al",             //  a + el
                  // es         from SER
                "lo",             //  him
                "como",           //  how
                "más",            //  more
                "pero",           //  pero
                "sus",            //  su plural
                "le",             //  to him, her
                "ya",             //  already
                "o",              //  or
                  // fue        from SER
                "este",           //  this
                  // ha         from HABER
                "sí",             //  himself etc
                "porque",         //  because
                "esta",           //  this
                  // son        from SER
                "entre",          //  between
                  // está       from ESTAR
                "cuando",         //  when
                "muy",            //  very
                "sin",            //  without
                "sobre",          //  on
                  // ser        from SER
                  // tiene      from TENER
                "también",        //  also
                "me",             //  me
                "hasta",          //  until
                "hay",            //  there is/are
                "donde",          //  where
                  // han        from HABER
                "quien",          //  whom, that
                  // están      from ESTAR
                  // estado     from ESTAR
                "desde",          //  from
                "todo",           //  all
                "nos",            //  us
                "durante",        //  during
                  // estados    from ESTAR
                "todos",          //  all
                "uno",            //  a
                "les",            //  to them
                "ni",             //  nor
                "contra",         //  against
                "otros",          //  other
                  // fueron     from SER
                "ese",            //  that
                "eso",            //  that
                  // había      from HABER
                "ante",           //  before
                "ellos",          //  they
                "e",              //  and (variant of y)
                "esto",           //  this
                "mí",             //  me
                "antes",          //  before
                "algunos",        //  some
                "qué",            //  what?
                "unos",           //  a
                "yo",             //  I
                "otro",           //  other
                "otras",          //  other
                "otra",           //  other
                "él",             //  he
                "tanto",          //  so much, many
                "esa",            //  that
                "estos",          //  these
                "mucho",          //  much, many
                "quienes",        //  who
                "nada",           //  nothing
                "muchos",         //  many
                "cual",           //  who
                  // sea        from SER
                "poco",           //  few
                "ella",           //  she
                "estar",          //  to be
                  // haber      from HABER
                "estas",          //  these
                  // estaba     from ESTAR
                  // estamos    from ESTAR
                "algunas",        //  some
                "algo",           //  something
                "nosotros",       //  we
                               // other forms
                "mi",             //  me
                "mis",            //  mi plural
                "tú",             //  thou
                "te",             //  thee
                "ti",             //  thee
                "tu",             //  thy
                "tus",            //  tu plural
                "ellas",          //  they
                "nosotras",       //  we
                "vosotros",       //  you
                "vosotras",       //  you
                "os",             //  you
                "mío",            //  mine
                "mía",            //
                "míos",           //
                "mías",           //
                "tuyo",           //  thine
                "tuya",           //
                "tuyos",          //
                "tuyas",          //
                "suyo",           //  his, hers, theirs
                "suya",           //
                "suyos",          //
                "suyas",          //
                "nuestro",        //  ours
                "nuestra",        //
                "nuestros",       //
                "nuestras",       //
                "vuestro",        //  yours
                "vuestra",        //
                "vuestros",       //
                "vuestras",       //
                "esos",           //  those
                "esas",           //  those
                               // forms of estar, to be (not including the infinitive):
                "estoy",
                "estás",
                "está",
                "estamos",
                "estáis",
                "están",
                "esté",
                "estés",
                "estemos",
                "estéis",
                "estén",
                "estaré",
                "estarás",
                "estará",
                "estaremos",
                "estaréis",
                "estarán",
                "estaría",
                "estarías",
                "estaríamos",
                "estaríais",
                "estarían",
                "estaba",
                "estabas",
                "estábamos",
                "estabais",
                "estaban",
                "estuve",
                "estuviste",
                "estuvo",
                "estuvimos",
                "estuvisteis",
                "estuvieron",
                "estuviera",
                "estuvieras",
                "estuviéramos",
                "estuvierais",
                "estuvieran",
                "estuviese",
                "estuvieses",
                "estuviésemos",
                "estuvieseis",
                "estuviesen",
                "estando",
                "estado",
                "estada",
                "estados",
                "estadas",
                "estad",
                               // forms of haber, to have (not including the infinitive):
                "he",
                "has",
                "ha",
                "hemos",
                "habéis",
                "han",
                "haya",
                "hayas",
                "hayamos",
                "hayáis",
                "hayan",
                "habré",
                "habrás",
                "habrá",
                "habremos",
                "habréis",
                "habrán",
                "habría",
                "habrías",
                "habríamos",
                "habríais",
                "habrían",
                "había",
                "habías",
                "habíamos",
                "habíais",
                "habían",
                "hube",
                "hubiste",
                "hubo",
                "hubimos",
                "hubisteis",
                "hubieron",
                "hubiera",
                "hubieras",
                "hubiéramos",
                "hubierais",
                "hubieran",
                "hubiese",
                "hubieses",
                "hubiésemos",
                "hubieseis",
                "hubiesen",
                "habiendo",
                "habido",
                "habida",
                "habidos",
                "habidas",
                               // forms of ser, to be (not including the infinitive):
                "soy",
                "eres",
                "es",
                "somos",
                "sois",
                "son",
                "sea",
                "seas",
                "seamos",
                "seáis",
                "sean",
                "seré",
                "serás",
                "será",
                "seremos",
                "seréis",
                "serán",
                "sería",
                "serías",
                "seríamos",
                "seríais",
                "serían",
                "era",
                "eras",
                "éramos",
                "erais",
                "eran",
                "fui",
                "fuiste",
                "fue",
                "fuimos",
                "fuisteis",
                "fueron",
                "fuera",
                "fueras",
                "fuéramos",
                "fuerais",
                "fueran",
                "fuese",
                "fueses",
                "fuésemos",
                "fueseis",
                "fuesen",
                "siendo",
                "sido",
                  //  sed       also means 'thirst'
                               // forms of tener, to have (not including the infinitive):
                "tengo",
                "tienes",
                "tiene",
                "tenemos",
                "tenéis",
                "tienen",
                "tenga",
                "tengas",
                "tengamos",
                "tengáis",
                "tengan",
                "tendré",
                "tendrás",
                "tendrá",
                "tendremos",
                "tendréis",
                "tendrán",
                "tendría",
                "tendrías",
                "tendríamos",
                "tendríais",
                "tendrían",
                "tenía",
                "tenías",
                "teníamos",
                "teníais",
                "tenían",
                "tuve",
                "tuviste",
                "tuvo",
                "tuvimos",
                "tuvisteis",
                "tuvieron",
                "tuviera",
                "tuvieras",
                "tuviéramos",
                "tuvierais",
                "tuvieran",
                "tuviese",
                "tuvieses",
                "tuviésemos",
                "tuvieseis",
                "tuviesen",
                "teniendo",
                "tenido",
                "tenida",
                "tenidos",
                "tenidas",
                "tened"}));

        // this list is taken from http://snowball.tartarus.org/algorithms/german/stop.txt
        public static Set<string>.ReadOnly GermanStopWords
            = new Set<string>.ReadOnly(new Set<string>(new string[] {
                "aber",           //  but
                "alle",           //  all
                "allem",
                "allen",
                "aller",
                "alles",
                "als",            //  than, as
                "also",           //  so
                "am",             //  an + dem
                "an",             //  at
                "ander",          //  other
                "andere",
                "anderem",
                "anderen",
                "anderer",
                "anderes",
                "anderm",
                "andern",
                "anderr",
                "anders",
                "auch",           //  also
                "auf",            //  on
                "aus",            //  out of
                "bei",            //  by
                "bin",            //  am
                "bis",            //  until
                "bist",           //  art
                "da",             //  there
                "damit",          //  with it
                "dann",           //  then
                "der",            //  the
                "den",
                "des",
                "dem",
                "die",
                "das",
                "daß",            //  that
                "derselbe",       //  the same
                "derselben",
                "denselben",
                "desselben",
                "demselben",
                "dieselbe",
                "dieselben",
                "dasselbe",
                "dazu",           //  to that
                "dein",           //  thy
                "deine",
                "deinem",
                "deinen",
                "deiner",
                "deines",
                "denn",           //  because
                "derer",          //  of those
                "dessen",         //  of him
                "dich",           //  thee
                "dir",            //  to thee
                "du",             //  thou
                "dies",           //  this
                "diese",
                "diesem",
                "diesen",
                "dieser",
                "dieses",
                "doch",           //  (several meanings)
                "dort",           //  (over) there
                "durch",          //  through
                "ein",            //  a
                "eine",
                "einem",
                "einen",
                "einer",
                "eines",
                "einig",          //  some
                "einige",
                "einigem",
                "einigen",
                "einiger",
                "einiges",
                "einmal",         //  once
                "er",             //  he
                "ihn",            //  him
                "ihm",            //  to him
                "es",             //  it
                "etwas",          //  something
                "euer",           //  your
                "eure",
                "eurem",
                "euren",
                "eurer",
                "eures",
                "für",            //  for
                "fuer",
                "gegen",          //  towards
                "gewesen",        //  p.p. of sein
                "hab",            //  have
                "habe",           //  have
                "haben",          //  have
                "hat",            //  has
                "hatte",          //  had
                "hatten",         //  had
                "hier",           //  here
                "hin",            //  there
                "hinter",         //  behind
                "ich",            //  I
                "mich",           //  me
                "mir",            //  to me
                "ihr",            //  you, to her
                "ihre",
                "ihrem",
                "ihren",
                "ihrer",
                "ihres",
                "euch",           //  to you
                "im",             //  in + dem
                "in",             //  in
                "indem",          //  while
                "ins",            //  in + das
                "ist",            //  is
                "jede",           //  each, every
                "jedem",
                "jeden",
                "jeder",
                "jedes",
                "jene",           //  that
                "jenem",
                "jenen",
                "jener",
                "jenes",
                "jetzt",          //  now
                "kann",           //  can
                "kein",           //  no
                "keine",
                "keinem",
                "keinen",
                "keiner",
                "keines",
                "können",         //  can
                "koennen",
                "könnte",         //  could
                "koennte",
                "machen",         //  do
                "man",            //  one
                "manche",         //  some, many a
                "manchem",
                "manchen",
                "mancher",
                "manches",
                "mein",           //  my
                "meine",
                "meinem",
                "meinen",
                "meiner",
                "meines",
                "mit",            //  with
                "muss",           //  must
                "musste",         //  had to
                "nach",           //  to(wards)
                "nicht",          //  not
                "nichts",         //  nothing
                "noch",           //  still, yet
                "nun",            //  now
                "nur",            //  only
                "ob",             //  whether
                "oder",           //  or
                "ohne",           //  without
                "sehr",           //  very
                "sein",           //  his
                "seine",
                "seinem",
                "seinen",
                "seiner",
                "seines",
                "selbst",         //  self
                "sich",           //  herself
                "sie",            //  they, she
                "ihnen",          //  to them
                "sind",           //  are
                "so",             //  so
                "solche",         //  such
                "solchem",
                "solchen",
                "solcher",
                "solches",
                "soll",           //  shall
                "sollte",         //  should
                "sondern",        //  but
                "sonst",          //  else
                "über",           //  over
                "ueber",
                "um",             //  about, around
                "und",            //  and
                "uns",            //  us
                "unse",
                "unsem",
                "unsen",
                "unser",
                "unses",
                "unter",          //  under
                "viel",           //  much
                "vom",            //  von + dem
                "von",            //  from
                "vor",            //  before
                "während",        //  while
                "waehrend",
                "war",            //  was
                "waren",          //  were
                "warst",          //  wast
                "was",            //  what
                "weg",            //  away, off
                "weil",           //  because
                "weiter",         //  further
                "welche",         //  which
                "welchem",
                "welchen",
                "welcher",
                "welches",
                "wenn",           //  when
                "werde",          //  will
                "werden",         //  will
                "wie",            //  how
                "wieder",         //  again
                "will",           //  want
                "wir",            //  we
                "wird",           //  will
                "wirst",          //  willst
                "wo",             //  where
                "wollen",         //  want
                "wollte",         //  wanted
                "würde",          //  would
                "wuerde",
                "würden",         //  would
                "wuerden",
                "zu",             //  to
                "zum",            //  zu + dem
                "zur",            //  zu + der
                "zwar",           //  indeed
                "zwischen"}));    //  between
    }
}
