# Evaluation

Mode: full (retrieval + answer) · Embedding model: `bge-m3` · Chat model: `qwen2.5:3b` · Top K: 5 · MinSimilarity: 0.420 · Run: 2026-11-20 01:04 UTC

Test split not run: it is held out and measured once, at the end, with `--final`.

| Metric | Tuning | Test |
|---|---|---|
| Questions (answerable) | 40 (32) | not run |
| Recall@5 | 80 % | not run |
| Hit rate@5 | 94 % | not run |
| MRR | 0.721 | not run |
| Correct rejection (threshold only) | 80 % | not run |
| Correct rejection (end to end) | 95 % | not run |
| Citation precision | 69 % | not run |
| Citations discarded by validator | 0 | not run |
| Min top similarity, answerable | 0.439 | not run |
| Max top similarity, should reject | 0.641 | not run |
| Mean latency (s) | 31.360 | not run |

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
| q001 | tuning | clear | cc-1985, cc-2028 | 1 | 0.488 | yes | cc-2028, cc-1985 | 45.880 | ok |
| q002 | tuning | everyday_words | cc-1985, cc-2028 | 1 | 0.587 | yes | cc-2028 | 19.931 | ok |
| q003 | tuning | everyday_words | cc-2029, cc-1998 | 2 | 0.563 | yes | cc-2029 | 20.840 | ok |
| q004 | tuning | everyday_words | cc-2029 | 2 | 0.567 | yes | cc-2029 | 22.300 | ok |
| q005 | tuning | clear | cc-1998, d1077-2.2.6.1.1.10 | 1 | 0.733 | yes | d1077-2.2.6.1.1.10 | 33.661 | ok |
| q006 | tuning | clear | l820-27, cc-1993 | 1 | 0.678 | yes | l820-27 | 34.127 | ok |
| q007 | tuning | everyday_words | l820-20 | 1 | 0.686 | yes | l820-20 | 37.223 | ok |
| q008 | tuning | clear | l820-16 | 1 | 0.749 | yes | l820-16 | 37.503 | ok |
| q009 | tuning | everyday_words | l820-17, cc-2004 | 1 | 0.655 | yes | l820-17 | 29.687 | ok |
| q010 | tuning | everyday_words | l820-24, l820-25 | 1 | 0.653 | yes | l820-24 | 43.749 | ok |
| q011 | tuning | clear | l820-8, cc-1982 | 2 | 0.730 | yes | l820-9, l820-8, cc-1985, cc-1982 | 38.494 | ok |
| q012 | tuning | everyday_words | cc-2028, cc-1985 | 1 | 0.551 | yes | cc-2028 | 21.206 | ok |
| q013 | tuning | repealed | l820-22 | 2 | 0.693 | yes | l820-22 | 33.779 | ok |
| q014 | tuning | out_of_corpus | (reject) | – | 0.591 | no | – | 37.144 | ok |
| q015 | tuning | out_of_corpus | (reject) | – | 0.500 | yes | l820-18 | 31.760 | FAIL |
| q016 | tuning | everyday_words | cc-2029, cc-1998 | 3 | 0.483 | yes | cc-2029 | 30.534 | ok |
| q017 | tuning | everyday_words | cc-1985, cc-2028 | 2 | 0.471 | yes | cc-1993 | 24.084 | FAIL |
| q018 | tuning | everyday_words | cc-2028, l820-9 | – | 0.536 | yes | cc-1985 | 29.548 | FAIL |
| q019 | tuning | clear | l820-9 | 2 | 0.771 | yes | l820-9 | 53.226 | ok |
| q020 | tuning | clear | cc-1990, l820-24 | 2 | 0.631 | yes | l820-21 | 30.828 | FAIL |
| q021 | tuning | everyday_words | cc-1993, l820-27 | 1 | 0.611 | yes | cc-1993 | 30.545 | ok |
| q022 | tuning | clear | l820-22, cc-2025 | 2 | 0.627 | yes | l820-22 | 34.919 | ok |
| q023 | tuning | clear | cc-2020, cc-2019 | 1 | 0.572 | yes | cc-2020 | 35.678 | ok |
| q024 | tuning | everyday_words | cc-2013, cc-2003, l820-25 | 1 | 0.567 | yes | cc-2013 | 42.290 | ok |
| q025 | tuning | everyday_words | cc-2032 | 1 | 0.633 | yes | cc-2032 | 27.187 | ok |
| q026 | tuning | clear | l820-23, l820-22 | 2 | 0.751 | yes | l820-24, l820-22 | 14.868 | ok |
| q027 | tuning | everyday_words | l820-22, l820-15 | 2 | 0.622 | yes | l820-24 | 43.183 | FAIL |
| q028 | tuning | clear | l820-13 | 1 | 0.715 | yes | l820-13 | 39.466 | ok |
| q029 | tuning | clear | l820-19 | 2 | 0.697 | yes | l820-19 | 30.004 | ok |
| q030 | tuning | clear | l820-28 | 1 | 0.679 | yes | l820-28 | 30.045 | ok |
| q031 | tuning | everyday_words | cc-1987, l820-8 | 4 | 0.580 | yes | cc-1988 | 31.382 | FAIL |
| q032 | tuning | clear | cc-1997, l820-9 | 1 | 0.686 | yes | l820-9, cc-1986, cc-1997, cc-2030 | 25.074 | ok |
| q033 | tuning | everyday_words | cc-1985, cc-2028, d1077-2.2.6.1.1.10 | 1 | 0.439 | yes | cc-1998 | 24.815 | FAIL |
| q034 | tuning | repealed | l820-22 | – | 0.621 | yes | cc-2013 | 26.579 | FAIL |
| q035 | tuning | repealed | (reject) | – | 0.542 | no | – | 30.827 | ok |
| q036 | tuning | repealed | (reject) | – | 0.641 | yes | l820-18 | 28.796 | FAIL |
| q037 | tuning | out_of_corpus | (reject) | – | 0.476 | no | – | 20.220 | ok |
| q038 | tuning | out_of_corpus | (reject) | – | 0.515 | no | – | 32.337 | ok |
| q039 | tuning | out_of_corpus | (reject) | – | 0.561 | no | – | 27.554 | ok |
| q040 | tuning | out_of_corpus | (reject) | – | 0.468 | no | – | 23.135 | ok |

## Failed questions

### q015: ¿Cuál es la cuota de administración máxima que puede cobrar un conjunto residencial?

Expected: reject · Basis: yes

1. `l820-18` 0.500
2. `l820-20` 0.492
3. `l820-15#3` 0.488
4. `l820-13` 0.477
5. `l820-14` 0.469

Answer (grounded: yes, cited: l820-18): La cuota de administración máxima que puede cobrar un conjunto residencial es el uno por ciento (1%) del valor comercial del inmueble o de la parte de él que se dé en arriendo, según lo establecido en la Ley 820 de 2003.

### q017: Se rompió una tubería dentro de la pared por lo vieja, ¿quién la paga?

Expected: cc-1985, cc-2028 · Basis: yes

1. `cc-2003` 0.471
2. `cc-2028` 0.452 ✓
3. `cc-1998` 0.451
4. `l820-15#3` 0.447
5. `cc-1993` 0.444

Answer (grounded: yes, cited: cc-1993): El arrendador es quien debe pagar las reparaciones no locativas si la tubería se rompió por la vetustez del edificio.

### q018: Cuando entregue el apartamento, ¿me toca pintarlo?

Expected: cc-2028, l820-9 · Basis: yes

1. `cc-2029` 0.536
2. `cc-2002` 0.524
3. `cc-2030` 0.514
4. `cc-1985` 0.510
5. `l820-24#2` 0.509

Answer (grounded: yes, cited: cc-1985): No, no es necesario que pinte el apartamento cuando lo entregues. La obligación de mantener la cosa arrendada en buen estado incluye hacer reparaciones necesarias durante el arriendo, pero no las reparaciones locativas que corresponden generalmente al arrendatario.

### q020: Si la vivienda está en tan mal estado que no se puede habitar, ¿puedo terminar el contrato?

Expected: cc-1990, l820-24 · Basis: yes

1. `l820-21` 0.631
2. `cc-1990` 0.624 ✓
3. `cc-2026` 0.585
4. `cc-1983` 0.584
5. `l820-22#1` 0.580

Answer (grounded: yes, cited: l820-21): Si la vivienda está en un estado tan mal que no se puede habituar, puedes terminar el contrato de acuerdo con la Ley 820 de 2003, artículo 21, que permite la terminación por mutuo acuerdo. Además, si el arrendador no ha cancelado las rentas y reajustes dentro del término estipulado, puedes pedir la terminación unilateralmente.

### q027: No pagué el agua y la luz y me las cortaron, ¿el dueño puede terminar el contrato?

Expected: l820-22, l820-15 · Basis: yes

1. `l820-24#1` 0.622
2. `l820-22#1` 0.618 ✓
3. `l820-21` 0.589
4. `l820-15#3` 0.587 ✓
5. `cc-1990` 0.586

Answer (grounded: yes, cited: l820-24): Según la Ley 820 de 2003, artículo 24, si el arrendatario incurre en mora en pagos que estén a su cargo, como el pago de servicios públicos, el arrendador puede pedir unilateralmente la terminación del contrato. En este caso, el arrendador podría terminar el contrato por no pagar el agua y la luz.

### q031: El dueño entra al apartamento sin permiso y me molesta, ¿tengo derecho a una indemnización?

Expected: cc-1987, l820-8 · Basis: yes

1. `cc-1988` 0.580
2. `cc-2003` 0.574
3. `cc-2030` 0.573
4. `cc-1987` 0.571 ✓
5. `cc-1986` 0.564

Answer (grounded: yes, cited: cc-1988): Sí, tienes derecho a una indemnización por el daño que has sufrido debido a que el dueño ha entrado sin permiso y te ha molestado.

### q033: Se cae el taco de la luz por una falla de la instalación eléctrica vieja, ¿quién la arregla?

Expected: cc-1985, cc-2028, d1077-2.2.6.1.1.10 · Basis: yes

1. `cc-2028` 0.439 ✓
2. `cc-1997` 0.426
3. `cc-1998` 0.426
4. `cc-1988` 0.419
5. `cc-2033` 0.415

Answer (grounded: yes, cited: cc-1998): El arrendatario es responsable de las reparaciones locativas, lo que incluye reparaciones por fallas en instalaciones eléctricas. En este caso, la caída del taco de la luz debido a una falla de la instalación eléctrica vieja es una reparación que corresponde al arrendatario.

### q034: ¿Qué pasa si me atraso un periodo entero en el pago de la renta según el Código Civil?

Expected: l820-22 · Basis: yes

1. `cc-2013` 0.621
2. `cc-2003` 0.619
3. `cc-2002` 0.606
4. `cc-2034` 0.600
5. `cc-1984` 0.578

Answer (grounded: yes, cited: cc-2013): Si usted se atrasa un periodo entero en el pago de la renta, el arrendatario será obligado a pagar la renta de todos los días que falten para que cese, aunque voluntariamente restituya la cosa antes del último día. Este pago se debe hacer según lo estipulado en el Código Civil, que establece que la renta de predios urbanos se paga por meses.

### q036: ¿Cómo se determina la cuantía de los procesos de tenencia por arrendamiento?

Expected: reject · Basis: yes

1. `l820-18` 0.641
2. `l820-5` 0.621
3. `cc-2027` 0.601
4. `l820-4#1` 0.601
5. `cc-2002` 0.600

Answer (grounded: yes, cited: l820-18): La cuantía de los procesos de tenencia por arrendamiento se determina por las partes en un porcentaje del valor comercial del inmueble o de la parte que se arrienda, no excediendo el uno por ciento (1%) del valor del inmueble o de la parte que se arrienda, según la Ley 820 de 2003, artículo 18.

