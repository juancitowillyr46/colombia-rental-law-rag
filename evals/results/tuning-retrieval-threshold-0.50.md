# Evaluation

Mode: retrieval only · Embedding model: `bge-m3` · Top K: 5 · MinSimilarity: 0.500 · Run: 2026-11-20 00:43 UTC

Test split not run: it is held out and measured once, at the end, with `--final`.

| Metric | Tuning | Test |
|---|---|---|
| Questions (answerable) | 40 (32) | not run |
| Recall@5 | 80 % | not run |
| Hit rate@5 | 94 % | not run |
| MRR | 0.721 | not run |
| Correct rejection (threshold only) | 78 % | not run |
| Min top similarity, answerable | 0.439 | not run |
| Max top similarity, should reject | 0.641 | not run |
| Mean latency (s) | 0.161 | not run |

## Threshold sweep (tuning only)

Threshold-only decision: false reject = answerable question below the threshold; false accept = question that should be rejected at or above it.

| MinSimilarity | Accuracy | False rejects | False accepts |
|---|---|---|---|
| 0.350 | 80 % | 0 | 8 |
| 0.370 | 80 % | 0 | 8 |
| 0.390 | 80 % | 0 | 8 |
| 0.410 | 80 % | 0 | 8 |
| 0.430 | 80 % | 0 | 8 |
| 0.450 | 78 % | 1 | 8 |
| 0.470 | 80 % | 1 | 7 |
| 0.490 | 75 % | 4 | 6 |
| 0.510 | 78 % | 4 | 5 |
| 0.530 | 80 % | 4 | 4 |
| 0.550 | 80 % | 5 | 3 |
| 0.570 | 72 % | 9 | 2 |
| 0.590 | 65 % | 12 | 2 |
| 0.610 | 68 % | 12 | 1 |
| 0.630 | 57 % | 16 | 1 |
| 0.650 | 55 % | 18 | 0 |

## Per question

| Id | Split | Category | Expected | Rank | Top similarity | Basis | Result |
|---|---|---|---|---|---|---|---|
| q001 | tuning | clear | cc-1985, cc-2028 | 1 | 0.488 | no | FAIL |
| q002 | tuning | everyday_words | cc-1985, cc-2028 | 1 | 0.587 | yes | ok |
| q003 | tuning | everyday_words | cc-2029, cc-1998 | 2 | 0.563 | yes | ok |
| q004 | tuning | everyday_words | cc-2029 | 2 | 0.567 | yes | ok |
| q005 | tuning | clear | cc-1998, d1077-2.2.6.1.1.10 | 1 | 0.733 | yes | ok |
| q006 | tuning | clear | l820-27, cc-1993 | 1 | 0.678 | yes | ok |
| q007 | tuning | everyday_words | l820-20 | 1 | 0.686 | yes | ok |
| q008 | tuning | clear | l820-16 | 1 | 0.749 | yes | ok |
| q009 | tuning | everyday_words | l820-17, cc-2004 | 1 | 0.655 | yes | ok |
| q010 | tuning | everyday_words | l820-24, l820-25 | 1 | 0.653 | yes | ok |
| q011 | tuning | clear | l820-8, cc-1982 | 2 | 0.730 | yes | ok |
| q012 | tuning | everyday_words | cc-2028, cc-1985 | 1 | 0.551 | yes | ok |
| q013 | tuning | repealed | l820-22 | 2 | 0.693 | yes | ok |
| q014 | tuning | out_of_corpus | (reject) | – | 0.591 | yes | FAIL |
| q015 | tuning | out_of_corpus | (reject) | – | 0.500 | no | ok |
| q016 | tuning | everyday_words | cc-2029, cc-1998 | 3 | 0.483 | no | FAIL |
| q017 | tuning | everyday_words | cc-1985, cc-2028 | 2 | 0.471 | no | FAIL |
| q018 | tuning | everyday_words | cc-2028, l820-9 | – | 0.536 | yes | FAIL |
| q019 | tuning | clear | l820-9 | 2 | 0.771 | yes | ok |
| q020 | tuning | clear | cc-1990, l820-24 | 2 | 0.631 | yes | ok |
| q021 | tuning | everyday_words | cc-1993, l820-27 | 1 | 0.611 | yes | ok |
| q022 | tuning | clear | l820-22, cc-2025 | 2 | 0.627 | yes | ok |
| q023 | tuning | clear | cc-2020, cc-2019 | 1 | 0.572 | yes | ok |
| q024 | tuning | everyday_words | cc-2013, cc-2003, l820-25 | 1 | 0.567 | yes | ok |
| q025 | tuning | everyday_words | cc-2032 | 1 | 0.633 | yes | ok |
| q026 | tuning | clear | l820-23, l820-22 | 2 | 0.751 | yes | ok |
| q027 | tuning | everyday_words | l820-22, l820-15 | 2 | 0.622 | yes | ok |
| q028 | tuning | clear | l820-13 | 1 | 0.715 | yes | ok |
| q029 | tuning | clear | l820-19 | 2 | 0.697 | yes | ok |
| q030 | tuning | clear | l820-28 | 1 | 0.679 | yes | ok |
| q031 | tuning | everyday_words | cc-1987, l820-8 | 4 | 0.580 | yes | ok |
| q032 | tuning | clear | cc-1997, l820-9 | 1 | 0.686 | yes | ok |
| q033 | tuning | everyday_words | cc-1985, cc-2028, d1077-2.2.6.1.1.10 | 1 | 0.439 | no | FAIL |
| q034 | tuning | repealed | l820-22 | – | 0.621 | yes | FAIL |
| q035 | tuning | repealed | (reject) | – | 0.542 | yes | FAIL |
| q036 | tuning | repealed | (reject) | – | 0.641 | yes | FAIL |
| q037 | tuning | out_of_corpus | (reject) | – | 0.476 | no | ok |
| q038 | tuning | out_of_corpus | (reject) | – | 0.515 | yes | FAIL |
| q039 | tuning | out_of_corpus | (reject) | – | 0.561 | yes | FAIL |
| q040 | tuning | out_of_corpus | (reject) | – | 0.468 | no | ok |

## Failed questions

### q001: ¿Quién arregla una gotera causada por lo viejo del techo?

Expected: cc-1985, cc-2028 · Basis: no

1. `cc-2028` 0.488 ✓
2. `d1077-2.2.6.1.1.10` 0.488
3. `cc-1998` 0.472
4. `cc-1985` 0.445 ✓
5. `cc-1988` 0.427

### q014: ¿Cuánto IVA se paga por arrendar un local comercial?

Expected: reject · Basis: yes

1. `l820-18` 0.591
2. `l820-27` 0.510
3. `l820-28` 0.504
4. `l820-2` 0.502
5. `l820-20` 0.502

### q016: Se partió una baldosa del piso del baño, ¿la tengo que cambiar yo?

Expected: cc-2029, cc-1998 · Basis: no

1. `cc-2030` 0.483
2. `cc-2028` 0.465
3. `cc-1998` 0.463 ✓
4. `cc-2029` 0.455 ✓
5. `d1077-2.2.6.1.1.10` 0.446

### q017: Se rompió una tubería dentro de la pared por lo vieja, ¿quién la paga?

Expected: cc-1985, cc-2028 · Basis: no

1. `cc-2003` 0.471
2. `cc-2028` 0.452 ✓
3. `cc-1998` 0.451
4. `l820-15#3` 0.447
5. `cc-1993` 0.444

### q018: Cuando entregue el apartamento, ¿me toca pintarlo?

Expected: cc-2028, l820-9 · Basis: yes

1. `cc-2029` 0.536
2. `cc-2002` 0.524
3. `cc-2030` 0.514
4. `cc-1985` 0.510
5. `l820-24#2` 0.509

### q033: Se cae el taco de la luz por una falla de la instalación eléctrica vieja, ¿quién la arregla?

Expected: cc-1985, cc-2028, d1077-2.2.6.1.1.10 · Basis: no

1. `cc-2028` 0.439 ✓
2. `cc-1997` 0.426
3. `cc-1998` 0.426
4. `cc-1988` 0.419
5. `cc-2033` 0.415

### q034: ¿Qué pasa si me atraso un periodo entero en el pago de la renta según el Código Civil?

Expected: l820-22 · Basis: yes

1. `cc-2013` 0.621
2. `cc-2003` 0.619
3. `cc-2002` 0.606
4. `cc-2034` 0.600
5. `cc-1984` 0.578

### q035: ¿Se puede pedir una inspección judicial del inmueble antes de notificar la demanda de restitución?

Expected: reject · Basis: yes

1. `cc-2007` 0.542
2. `l820-25#2` 0.533
3. `cc-2006` 0.529
4. `l820-23#1` 0.522
5. `l820-26` 0.518

### q036: ¿Cómo se determina la cuantía de los procesos de tenencia por arrendamiento?

Expected: reject · Basis: yes

1. `l820-18` 0.641
2. `l820-5` 0.621
3. `cc-2027` 0.601
4. `l820-4#1` 0.601
5. `cc-2002` 0.600

### q038: ¿Cómo registro una empresa en la Cámara de Comercio?

Expected: reject · Basis: yes

1. `l820-30` 0.515
2. `l820-28` 0.497
3. `l820-29` 0.460
4. `l820-31` 0.447
5. `l820-10#2` 0.434

### q039: ¿Qué dice la ley sobre el arriendo de una finca para cultivar?

Expected: reject · Basis: yes

1. `cc-1973` 0.561
2. `cc-1974` 0.549
3. `cc-1996` 0.547
4. `cc-2020` 0.541
5. `cc-2002` 0.535

