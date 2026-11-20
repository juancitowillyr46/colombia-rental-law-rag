# Evaluation

Mode: full (retrieval + answer) · Embedding model: `bge-m3` · Chat model: `qwen2.5:7b` · Top K: 5 · MinSimilarity: 0.420 · Run: 2026-11-20 02:44 UTC

Test split not run: it is held out and measured once, at the end, with `--final`.

| Metric | Tuning | Test |
|---|---|---|
| Questions (answerable) | 40 (32) | not run |
| Recall@5 | 80 % | not run |
| Hit rate@5 | 94 % | not run |
| MRR | 0.721 | not run |
| Correct rejection (threshold only) | 80 % | not run |
| Correct rejection (end to end) | 93 % | not run |
| Citation precision | 73 % | not run |
| Citations discarded by validator | 0 | not run |
| Min top similarity, answerable | 0.439 | not run |
| Max top similarity, should reject | 0.641 | not run |
| Mean latency (s) | 69.757 | not run |

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

| Id | Split | Category | Expected | Rank | Top similarity | Grounded | Cited | Latency (s) | Result |
|---|---|---|---|---|---|---|---|---|---|
| q001 | tuning | clear | cc-1985, cc-2028 | 1 | 0.488 | yes | cc-2028 | 22.592 | ok |
| q002 | tuning | everyday_words | cc-1985, cc-2028 | 1 | 0.587 | yes | cc-2028 | 19.106 | ok |
| q003 | tuning | everyday_words | cc-2029, cc-1998 | 2 | 0.563 | yes | cc-2029 | 51.097 | ok |
| q004 | tuning | everyday_words | cc-2029 | 2 | 0.567 | yes | cc-2029 | 38.842 | ok |
| q005 | tuning | clear | cc-1998, d1077-2.2.6.1.1.10 | 1 | 0.733 | yes | d1077-2.2.6.1.1.10 | 89.905 | ok |
| q006 | tuning | clear | l820-27, cc-1993 | 1 | 0.678 | yes | l820-27 | 85.465 | ok |
| q007 | tuning | everyday_words | l820-20 | 1 | 0.686 | yes | l820-20 | 78.090 | ok |
| q008 | tuning | clear | l820-16 | 1 | 0.749 | yes | l820-16 | 87.802 | ok |
| q009 | tuning | everyday_words | l820-17, cc-2004 | 1 | 0.655 | yes | l820-17 | 66.907 | ok |
| q010 | tuning | everyday_words | l820-24, l820-25 | 1 | 0.653 | yes | l820-24 | 95.359 | ok |
| q011 | tuning | clear | l820-8, cc-1982 | 2 | 0.730 | yes | l820-8, l820-9 | 89.031 | ok |
| q012 | tuning | everyday_words | cc-2028, cc-1985 | 1 | 0.551 | yes | cc-2028 | 39.141 | ok |
| q013 | tuning | repealed | l820-22 | 2 | 0.693 | yes | l820-22 | 71.436 | ok |
| q014 | tuning | out_of_corpus | (reject) | – | 0.591 | no | – | 88.178 | ok |
| q015 | tuning | out_of_corpus | (reject) | – | 0.500 | no | – | 59.695 | ok |
| q016 | tuning | everyday_words | cc-2029, cc-1998 | 3 | 0.483 | yes | cc-2028 | 60.974 | FAIL |
| q017 | tuning | everyday_words | cc-1985, cc-2028 | 2 | 0.471 | yes | cc-2028 | 62.681 | ok |
| q018 | tuning | everyday_words | cc-2028, l820-9 | – | 0.536 | yes | cc-2029 | 102.756 | FAIL |
| q019 | tuning | clear | l820-9 | 2 | 0.771 | yes | l820-9 | 188.521 | ok |
| q020 | tuning | clear | cc-1990, l820-24 | 2 | 0.631 | yes | cc-1990 | 92.471 | ok |
| q021 | tuning | everyday_words | cc-1993, l820-27 | 1 | 0.611 | yes | cc-1993 | 100.844 | ok |
| q022 | tuning | clear | l820-22, cc-2025 | 2 | 0.627 | yes | cc-2025 | 77.125 | ok |
| q023 | tuning | clear | cc-2020, cc-2019 | 1 | 0.572 | yes | cc-2019 | 72.647 | ok |
| q024 | tuning | everyday_words | cc-2013, cc-2003, l820-25 | 1 | 0.567 | yes | cc-2013 | 81.358 | ok |
| q025 | tuning | everyday_words | cc-2032 | 1 | 0.633 | yes | cc-2032 | 59.094 | ok |
| q026 | tuning | clear | l820-23, l820-22 | 2 | 0.751 | yes | l820-24, l820-22 | 34.796 | ok |
| q027 | tuning | everyday_words | l820-22, l820-15 | 2 | 0.622 | yes | l820-22 | 90.230 | ok |
| q028 | tuning | clear | l820-13 | 1 | 0.715 | yes | l820-13, l820-4 | 85.235 | ok |
| q029 | tuning | clear | l820-19 | 2 | 0.697 | yes | l820-19, cc-2002 | 61.336 | ok |
| q030 | tuning | clear | l820-28 | 1 | 0.679 | yes | l820-28 | 71.694 | ok |
| q031 | tuning | everyday_words | cc-1987, l820-8 | 4 | 0.580 | yes | cc-1988 | 69.933 | FAIL |
| q032 | tuning | clear | cc-1997, l820-9 | 1 | 0.686 | yes | l820-9 | 54.075 | ok |
| q033 | tuning | everyday_words | cc-1985, cc-2028, d1077-2.2.6.1.1.10 | 1 | 0.439 | yes | cc-1998, cc-2028 | 59.668 | ok |
| q034 | tuning | repealed | l820-22 | – | 0.621 | yes | cc-1984 | 49.284 | FAIL |
| q035 | tuning | repealed | (reject) | – | 0.542 | no | – | 50.684 | ok |
| q036 | tuning | repealed | (reject) | – | 0.641 | yes | l820-18 | 52.805 | FAIL |
| q037 | tuning | out_of_corpus | (reject) | – | 0.476 | no | – | 45.051 | ok |
| q038 | tuning | out_of_corpus | (reject) | – | 0.515 | yes | l820-29, l820-28 | 78.382 | FAIL |
| q039 | tuning | out_of_corpus | (reject) | – | 0.561 | yes | cc-2020, cc-2002 | 51.141 | FAIL |
| q040 | tuning | out_of_corpus | (reject) | – | 0.468 | no | – | 54.828 | ok |

## Failed questions

### q016: Se partió una baldosa del piso del baño, ¿la tengo que cambiar yo?

Expected: cc-2029, cc-1998 · Basis: yes

1. `cc-2030` 0.483
2. `cc-2028` 0.465
3. `cc-1998` 0.463 ✓
4. `cc-2029` 0.455 ✓
5. `d1077-2.2.6.1.1.10` 0.446

Answer (grounded: yes, cited: cc-2028): Sí, debes cambiar la baldosa del piso del baño, ya que es una reparación locativa que corresponde al inquilino según el artículo 2028 del Código Civil.

### q018: Cuando entregue el apartamento, ¿me toca pintarlo?

Expected: cc-2028, l820-9 · Basis: yes

1. `cc-2029` 0.536
2. `cc-2002` 0.524
3. `cc-2030` 0.514
4. `cc-1985` 0.510
5. `l820-24#2` 0.509

Answer (grounded: yes, cited: cc-2029): Según el artículo [cc-2029] del Código Civil, el inquilino está obligado a mantener la integridad de las paredes y a mantener el edificio medianamente aseadas, pero no se especifica que el inquilino deba pintar el apartamento al dejarlo.

### q031: El dueño entra al apartamento sin permiso y me molesta, ¿tengo derecho a una indemnización?

Expected: cc-1987, l820-8 · Basis: yes

1. `cc-1988` 0.580
2. `cc-2003` 0.574
3. `cc-2030` 0.573
4. `cc-1987` 0.571 ✓
5. `cc-1986` 0.564

Answer (grounded: yes, cited: cc-1988): Sí, tienes derecho a una indemnización por el daño que te ocasiona el dueño entrando sin permiso, ya que esto constituye una turbación en tu goce del inmueble.

### q034: ¿Qué pasa si me atraso un periodo entero en el pago de la renta según el Código Civil?

Expected: l820-22 · Basis: yes

1. `cc-2013` 0.621
2. `cc-2003` 0.619
3. `cc-2002` 0.606
4. `cc-2034` 0.600
5. `cc-1984` 0.578

Answer (grounded: yes, cited: cc-1984): Según el Código Civil, si el arrendador es constituido en mora de entregar debido a tu atraso en el pago de la renta, tienes derecho a indemnización de perjuicios. Esto se establece en el artículo 1984 [cc-1984] del Código Civil.

### q036: ¿Cómo se determina la cuantía de los procesos de tenencia por arrendamiento?

Expected: reject · Basis: yes

1. `l820-18` 0.641
2. `l820-5` 0.621
3. `cc-2027` 0.601
4. `l820-4#1` 0.601
5. `cc-2002` 0.600

Answer (grounded: yes, cited: l820-18): La cuantía del arrendamiento no puede exceder el uno por ciento (1%) del valor comercial del inmueble o de la parte que se arrienda, y este valor comercial no puede superar dos veces el avalúo catastral vigente.

### q038: ¿Cómo registro una empresa en la Cámara de Comercio?

Expected: reject · Basis: yes

1. `l820-30` 0.515
2. `l820-28` 0.497
3. `l820-29` 0.460
4. `l820-31` 0.447
5. `l820-10#2` 0.434

Answer (grounded: yes, cited: l820-29, l820-28): Para registrar una empresa en la Cámara de Comercio, se debe cumplir con los requisitos establecidos en el artículo 29 de la Ley 820 de 2003, como presentar el registro mercantil en el caso de personas jurídicas y el modelo de contratos de arrendamiento. Además, se debe matricularse como arrendador según lo estipula el artículo 28 de la misma ley.

### q039: ¿Qué dice la ley sobre el arriendo de una finca para cultivar?

Expected: reject · Basis: yes

1. `cc-1973` 0.561
2. `cc-1974` 0.549
3. `cc-1996` 0.547
4. `cc-2020` 0.541
5. `cc-2002` 0.535

Answer (grounded: yes, cited: cc-2020, cc-2002): El arrendamiento de una finca para cultivar está permitido y se regula según las disposiciones del Código Civil. Se especifica que la renta de predios rústicos se paga anualmente.

