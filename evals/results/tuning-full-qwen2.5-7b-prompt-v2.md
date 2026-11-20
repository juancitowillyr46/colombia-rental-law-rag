# Evaluation

Mode: full (retrieval + answer) · Embedding model: `bge-m3` · Chat model: `qwen2.5:7b` · Top K: 5 · MinSimilarity: 0.420 · Run: 2026-11-20 03:49 UTC

Test split not run: it is held out and measured once, at the end, with `--final`.

| Metric | Tuning | Test |
|---|---|---|
| Questions (answerable) | 40 (32) | not run |
| Recall@5 | 80 % | not run |
| Hit rate@5 | 94 % | not run |
| MRR | 0.721 | not run |
| Correct rejection (threshold only) | 80 % | not run |
| Correct rejection (end to end) | 95 % | not run |
| Citation precision | 75 % | not run |
| Citations discarded by validator | 0 | not run |
| Min top similarity, answerable | 0.439 | not run |
| Max top similarity, should reject | 0.641 | not run |
| Mean latency (s) | 62.462 | not run |

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
| q001 | tuning | clear | cc-1985, cc-2028 | 1 | 0.488 | yes | cc-2028 | 89.118 | ok |
| q002 | tuning | everyday_words | cc-1985, cc-2028 | 1 | 0.587 | yes | cc-2028 | 44.305 | ok |
| q003 | tuning | everyday_words | cc-2029, cc-1998 | 2 | 0.563 | yes | cc-2029 | 50.117 | ok |
| q004 | tuning | everyday_words | cc-2029 | 2 | 0.567 | yes | cc-2029, cc-1998 | 41.822 | ok |
| q005 | tuning | clear | cc-1998, d1077-2.2.6.1.1.10 | 1 | 0.733 | yes | d1077-2.2.6.1.1.10 | 86.151 | ok |
| q006 | tuning | clear | l820-27, cc-1993 | 1 | 0.678 | yes | l820-27 | 80.201 | ok |
| q007 | tuning | everyday_words | l820-20 | 1 | 0.686 | yes | l820-20 | 77.184 | ok |
| q008 | tuning | clear | l820-16 | 1 | 0.749 | yes | l820-16 | 81.302 | ok |
| q009 | tuning | everyday_words | l820-17, cc-2004 | 1 | 0.655 | yes | l820-17 | 68.573 | ok |
| q010 | tuning | everyday_words | l820-24, l820-25 | 1 | 0.653 | yes | l820-24 | 90.893 | ok |
| q011 | tuning | clear | l820-8, cc-1982 | 2 | 0.730 | yes | l820-8, cc-1982 | 72.415 | ok |
| q012 | tuning | everyday_words | cc-2028, cc-1985 | 1 | 0.551 | yes | cc-2028 | 42.254 | ok |
| q013 | tuning | repealed | l820-22 | 2 | 0.693 | yes | cc-2013 | 63.450 | FAIL |
| q014 | tuning | out_of_corpus | (reject) | – | 0.591 | no | – | 75.393 | ok |
| q015 | tuning | out_of_corpus | (reject) | – | 0.500 | no | – | 54.785 | ok |
| q016 | tuning | everyday_words | cc-2029, cc-1998 | 3 | 0.483 | yes | cc-2028 | 57.420 | FAIL |
| q017 | tuning | everyday_words | cc-1985, cc-2028 | 2 | 0.471 | yes | cc-2028 | 49.943 | ok |
| q018 | tuning | everyday_words | cc-2028, l820-9 | – | 0.536 | yes | cc-2029 | 61.020 | FAIL |
| q019 | tuning | clear | l820-9 | 2 | 0.771 | yes | l820-9 | 101.712 | ok |
| q020 | tuning | clear | cc-1990, l820-24 | 2 | 0.631 | yes | cc-1990 | 50.222 | ok |
| q021 | tuning | everyday_words | cc-1993, l820-27 | 1 | 0.611 | yes | cc-1993 | 55.952 | ok |
| q022 | tuning | clear | l820-22, cc-2025 | 2 | 0.627 | yes | cc-2025 | 61.243 | ok |
| q023 | tuning | clear | cc-2020, cc-2019 | 1 | 0.572 | yes | cc-2019 | 71.844 | ok |
| q024 | tuning | everyday_words | cc-2013, cc-2003, l820-25 | 1 | 0.567 | yes | cc-2013 | 83.168 | ok |
| q025 | tuning | everyday_words | cc-2032 | 1 | 0.633 | yes | cc-2032 | 60.460 | ok |
| q026 | tuning | clear | l820-23, l820-22 | 2 | 0.751 | yes | l820-22, l820-24 | 30.142 | ok |
| q027 | tuning | everyday_words | l820-22, l820-15 | 2 | 0.622 | yes | l820-22 | 81.183 | ok |
| q028 | tuning | clear | l820-13 | 1 | 0.715 | yes | l820-13 | 75.403 | ok |
| q029 | tuning | clear | l820-19 | 2 | 0.697 | yes | l820-19 | 56.840 | ok |
| q030 | tuning | clear | l820-28 | 1 | 0.679 | yes | l820-28 | 61.167 | ok |
| q031 | tuning | everyday_words | cc-1987, l820-8 | 4 | 0.580 | yes | cc-1988 | 62.784 | FAIL |
| q032 | tuning | clear | cc-1997, l820-9 | 1 | 0.686 | yes | l820-9 | 48.643 | ok |
| q033 | tuning | everyday_words | cc-1985, cc-2028, d1077-2.2.6.1.1.10 | 1 | 0.439 | yes | cc-1998, cc-2028 | 53.221 | ok |
| q034 | tuning | repealed | l820-22 | – | 0.621 | yes | cc-2034 | 46.662 | FAIL |
| q035 | tuning | repealed | (reject) | – | 0.542 | no | – | 51.608 | ok |
| q036 | tuning | repealed | (reject) | – | 0.641 | no | – | 50.058 | ok |
| q037 | tuning | out_of_corpus | (reject) | – | 0.476 | no | – | 40.533 | ok |
| q038 | tuning | out_of_corpus | (reject) | – | 0.515 | yes | l820-28 | 75.406 | FAIL |
| q039 | tuning | out_of_corpus | (reject) | – | 0.561 | yes | cc-1974, cc-2020 | 47.733 | FAIL |
| q040 | tuning | out_of_corpus | (reject) | – | 0.468 | no | – | 46.155 | ok |

## Failed questions

### q013: Si me atraso en el pago del arriendo, ¿el dueño puede terminar el contrato?

Expected: l820-22 · Basis: yes

1. `cc-2003` 0.693
2. `l820-22#2` 0.688 ✓
3. `l820-24#2` 0.686
4. `cc-1990` 0.685
5. `cc-2013` 0.683

Answer (grounded: yes, cited: cc-2013): Según el artículo [cc-2013] del Código Civil, si el arrendamiento debe cesar debido a un atraso en el pago, el arrendador puede terminar el contrato. Sin embargo, el arrendatario deberá pagar la renta por el tiempo que falte hasta que se termine el arrendamiento.

### q016: Se partió una baldosa del piso del baño, ¿la tengo que cambiar yo?

Expected: cc-2029, cc-1998 · Basis: yes

1. `cc-2030` 0.483
2. `cc-2028` 0.465
3. `cc-1998` 0.463 ✓
4. `cc-2029` 0.455 ✓
5. `d1077-2.2.6.1.1.10` 0.446

Answer (grounded: yes, cited: cc-2028): Sí, debes cambiar la baldosa del piso del baño, ya que es una reparación locativa que corresponde al arrendatario según el artículo 2028 del Código Civil.

### q018: Cuando entregue el apartamento, ¿me toca pintarlo?

Expected: cc-2028, l820-9 · Basis: yes

1. `cc-2029` 0.536
2. `cc-2002` 0.524
3. `cc-2030` 0.514
4. `cc-1985` 0.510
5. `l820-24#2` 0.509

Answer (grounded: yes, cited: cc-2029): Según el artículo [cc-2029] del Código Civil, el inquilino está obligado a mantener la integridad interior del edificio, pero no se especifica que deba pintarlo. La obligación de mantener en buen estado la vivienda incluye reparaciones menores, pero no se menciona pintura en particular.

### q031: El dueño entra al apartamento sin permiso y me molesta, ¿tengo derecho a una indemnización?

Expected: cc-1987, l820-8 · Basis: yes

1. `cc-1988` 0.580
2. `cc-2003` 0.574
3. `cc-2030` 0.573
4. `cc-1987` 0.571 ✓
5. `cc-1986` 0.564

Answer (grounded: yes, cited: cc-1988): Sí, tienes derecho a una indemnización por la molestia causada, ya que el dueño entró sin permiso y esto te turbó en tu goce del apartamento.

### q034: ¿Qué pasa si me atraso un periodo entero en el pago de la renta según el Código Civil?

Expected: l820-22 · Basis: yes

1. `cc-2013` 0.621
2. `cc-2003` 0.619
3. `cc-2002` 0.606
4. `cc-2034` 0.600
5. `cc-1984` 0.578

Answer (grounded: yes, cited: cc-2034): Según el artículo 2034 del Código Civil, si el arrendador es constituido en mora de entregar debido a un atraso de un período entero en el pago de la renta, el arrendatario tiene derecho a indemnización de perjuicios.

### q038: ¿Cómo registro una empresa en la Cámara de Comercio?

Expected: reject · Basis: yes

1. `l820-30` 0.515
2. `l820-28` 0.497
3. `l820-29` 0.460
4. `l820-31` 0.447
5. `l820-10#2` 0.434

Answer (grounded: yes, cited: l820-28): Según la Ley 820 de 2003, artículo 28, toda persona natural o jurídica que arrendé bienes raíces destinados a vivienda urbana, de su propiedad o de la de terceros, en municipios de más de 15.000 habitantes, debe matricularse ante la autoridad administrativa competente. También deben matricularse aquellas personas que celebren más de cinco contratos de arrendamiento en un municipio.

### q039: ¿Qué dice la ley sobre el arriendo de una finca para cultivar?

Expected: reject · Basis: yes

1. `cc-1973` 0.561
2. `cc-1974` 0.549
3. `cc-1996` 0.547
4. `cc-2020` 0.541
5. `cc-2002` 0.535

Answer (grounded: yes, cited: cc-1974, cc-2020): El arrendamiento de una finca para cultivar está permitido y se regula según las disposiciones del Código Civil, específicamente en los artículos 1974 y 2020.

