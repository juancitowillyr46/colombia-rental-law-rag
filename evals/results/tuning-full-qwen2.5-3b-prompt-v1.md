# Evaluation

Mode: full (retrieval + answer) · Embedding model: `bge-m3` · Chat model: `qwen2.5:3b` · Top K: 5 · MinSimilarity: 0.420 · Run: 2026-11-20 03:05 UTC

Test split not run: it is held out and measured once, at the end, with `--final`.

| Metric | Tuning | Test |
|---|---|---|
| Questions (answerable) | 40 (32) | not run |
| Recall@5 | 80 % | not run |
| Hit rate@5 | 94 % | not run |
| MRR | 0.721 | not run |
| Correct rejection (threshold only) | 80 % | not run |
| Correct rejection (end to end) | 90 % | not run |
| Citation precision | 62 % | not run |
| Citations discarded by validator | 1 | not run |
| Min top similarity, answerable | 0.439 | not run |
| Max top similarity, should reject | 0.641 | not run |
| Mean latency (s) | 29.717 | not run |

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
| q001 | tuning | clear | cc-1985, cc-2028 | 1 | 0.488 | yes | cc-2028, cc-1985 | 39.101 | ok |
| q002 | tuning | everyday_words | cc-1985, cc-2028 | 1 | 0.587 | yes | cc-2028 | 19.317 | ok |
| q003 | tuning | everyday_words | cc-2029, cc-1998 | 2 | 0.563 | yes | cc-2029 | 19.894 | ok |
| q004 | tuning | everyday_words | cc-2029 | 2 | 0.567 | yes | cc-2029 | 20.832 | ok |
| q005 | tuning | clear | cc-1998, d1077-2.2.6.1.1.10 | 1 | 0.733 | yes | d1077-2.2.6.1.1.10 | 31.768 | ok |
| q006 | tuning | clear | l820-27, cc-1993 | 1 | 0.678 | yes | l820-27 | 35.460 | ok |
| q007 | tuning | everyday_words | l820-20 | 1 | 0.686 | yes | l820-20 | 32.838 | ok |
| q008 | tuning | clear | l820-16 | 1 | 0.749 | yes | l820-16 | 39.836 | ok |
| q009 | tuning | everyday_words | l820-17, cc-2004 | 1 | 0.655 | yes | l820-17 | 27.095 | ok |
| q010 | tuning | everyday_words | l820-24, l820-25 | 1 | 0.653 | yes | l820-24 | 43.871 | ok |
| q011 | tuning | clear | l820-8, cc-1982 | 2 | 0.730 | yes | l820-9, l820-8, cc-1985, cc-1982 | 33.673 | ok |
| q012 | tuning | everyday_words | cc-2028, cc-1985 | 1 | 0.551 | yes | cc-2028 | 19.662 | ok |
| q013 | tuning | repealed | l820-22 | 2 | 0.693 | yes | l820-22 | 31.665 | ok |
| q014 | tuning | out_of_corpus | (reject) | – | 0.591 | no | – | 34.478 | ok |
| q015 | tuning | out_of_corpus | (reject) | – | 0.500 | yes | l820-18 | 29.916 | FAIL |
| q016 | tuning | everyday_words | cc-2029, cc-1998 | 3 | 0.483 | yes | cc-2029 | 26.386 | ok |
| q017 | tuning | everyday_words | cc-1985, cc-2028 | 2 | 0.471 | yes | cc-1993 | 23.313 | FAIL |
| q018 | tuning | everyday_words | cc-2028, l820-9 | – | 0.536 | yes | cc-2029, cc-1985 | 28.569 | FAIL |
| q019 | tuning | clear | l820-9 | 2 | 0.771 | yes | l820-9 | 50.856 | ok |
| q020 | tuning | clear | cc-1990, l820-24 | 2 | 0.631 | yes | l820-21 | 29.727 | FAIL |
| q021 | tuning | everyday_words | cc-1993, l820-27 | 1 | 0.611 | yes | cc-1993 | 32.378 | ok |
| q022 | tuning | clear | l820-22, cc-2025 | 2 | 0.627 | yes | l820-22, cc-1990 | 42.017 | ok |
| q023 | tuning | clear | cc-2020, cc-2019 | 1 | 0.572 | no | – | 32.483 | FAIL |
| q024 | tuning | everyday_words | cc-2013, cc-2003, l820-25 | 1 | 0.567 | yes | cc-2013 | 38.689 | ok |
| q025 | tuning | everyday_words | cc-2032 | 1 | 0.633 | yes | cc-2032 | 25.557 | ok |
| q026 | tuning | clear | l820-23, l820-22 | 2 | 0.751 | yes | l820-24, l820-22 | 13.845 | ok |
| q027 | tuning | everyday_words | l820-22, l820-15 | 2 | 0.622 | yes | l820-24 | 43.092 | FAIL |
| q028 | tuning | clear | l820-13 | 1 | 0.715 | yes | l820-9 | 38.635 | FAIL |
| q029 | tuning | clear | l820-19 | 2 | 0.697 | yes | l820-19 | 31.020 | ok |
| q030 | tuning | clear | l820-28 | 1 | 0.679 | yes | l820-28 | 28.143 | ok |
| q031 | tuning | everyday_words | cc-1987, l820-8 | 4 | 0.580 | yes | cc-1988 | 28.613 | FAIL |
| q032 | tuning | clear | cc-1997, l820-9 | 1 | 0.686 | yes | l820-9, cc-1986, cc-1997, cc-2030 | 25.201 | ok |
| q033 | tuning | everyday_words | cc-1985, cc-2028, d1077-2.2.6.1.1.10 | 1 | 0.439 | yes | cc-1998 | 23.606 | FAIL |
| q034 | tuning | repealed | l820-22 | – | 0.621 | yes | cc-2013 | 22.262 | FAIL |
| q035 | tuning | repealed | (reject) | – | 0.542 | no | – | 23.894 | ok |
| q036 | tuning | repealed | (reject) | – | 0.641 | yes | l820-18 | 23.928 | FAIL |
| q037 | tuning | out_of_corpus | (reject) | – | 0.476 | no | – | 18.740 | ok |
| q038 | tuning | out_of_corpus | (reject) | – | 0.515 | no | – | 28.635 | ok |
| q039 | tuning | out_of_corpus | (reject) | – | 0.561 | no | – | 25.752 | ok |
| q040 | tuning | out_of_corpus | (reject) | – | 0.468 | yes | l820-19 | 23.917 | FAIL |

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

Answer (grounded: yes, cited: cc-1993): El propietario o arrendador debe pagar las reparaciones si la tubería se rompió por la edad del edificio, que es una causa justificada por defectos de construcción.

### q018: Cuando entregue el apartamento, ¿me toca pintarlo?

Expected: cc-2028, l820-9 · Basis: yes

1. `cc-2029` 0.536
2. `cc-2002` 0.524
3. `cc-2030` 0.514
4. `cc-1985` 0.510
5. `l820-24#2` 0.509

Answer (grounded: yes, cited: cc-2029, cc-1985): No, no es obligatorio que el inquilino pinte el apartamento cuando lo entregue. La obligación de mantener la cosa arrendada en buen estado incluye hacer reparaciones necesarias durante el arriendo, pero no se menciona específicamente que el inquilino tenga que pintar el apartamento.

### q020: Si la vivienda está en tan mal estado que no se puede habitar, ¿puedo terminar el contrato?

Expected: cc-1990, l820-24 · Basis: yes

1. `l820-21` 0.631
2. `cc-1990` 0.624 ✓
3. `cc-2026` 0.585
4. `cc-1983` 0.584
5. `l820-22#1` 0.580

Answer (grounded: yes, cited: l820-21): Si la vivienda está en un estado tan malo que no se puede habituar, puedes terminar el contrato de acuerdo con la Ley 820 de 2003, artículo 21, que permite la terminación por mutuo acuerdo. Además, si el arrendador no ha cancelado las rentas y reajustes dentro del término estipulado, puedes pedir la terminación unilateralmente.

### q023: Si el dueño vende la casa, ¿el nuevo dueño tiene que respetar mi contrato?

Expected: cc-2020, cc-2019 · Basis: yes

1. `cc-2020` 0.572 ✓
2. `cc-2014` 0.552
3. `l820-17` 0.536
4. `cc-1996` 0.536
5. `cc-2019` 0.530 ✓

Answer (grounded: no, cited: –): No encontré base suficiente en las normas consultadas (Ley 820 de 2003, Código Civil y Decreto 1077 de 2015) para responder esa pregunta. Si tu caso es particular, consulta con un abogado o con la oficina de vivienda de tu municipio.

### q027: No pagué el agua y la luz y me las cortaron, ¿el dueño puede terminar el contrato?

Expected: l820-22, l820-15 · Basis: yes

1. `l820-24#1` 0.622
2. `l820-22#1` 0.618 ✓
3. `l820-21` 0.589
4. `l820-15#3` 0.587 ✓
5. `cc-1990` 0.586

Answer (grounded: yes, cited: l820-24): Según la Ley 820 de 2003, artículo 24, el arrendatario puede dar por terminado unilateralmente el contrato si el arrendador incurre en procederes que afecten gravemente el disfrute del inmueble, debidamente comprobados ante la autoridad policial. No pagarte el agua y la luz podría considerarse un proceder que afecte el disfrute del inmueble, por lo que el dueño podría terminar el contrato si cumple con los requisitos establecidos.

### q028: ¿Qué reglas aplican a quienes viven en una pensión o vivienda compartida?

Expected: l820-13 · Basis: yes

1. `l820-13` 0.715 ✓
2. `cc-2002` 0.541
3. `l820-4#1` 0.523
4. `l820-9` 0.516
5. `cc-2020` 0.511

Answer (grounded: yes, cited: l820-9): Las personas que viven en una pensión o vivienda compartida están sujetas a las reglas establecidas en el artículo 9 de la Ley 820 de 2003, que incluye pagar el precio del arrendamiento dentro del plazo estipulado, cuidar el inmueble y las cosas recibidas en arrendamiento, pagar a tiempo los servicios y cosas adicionales, cumplir las normas de propiedad horizontal y realizar reparaciones cuando sean atribuibles a su propia culpa.

### q031: El dueño entra al apartamento sin permiso y me molesta, ¿tengo derecho a una indemnización?

Expected: cc-1987, l820-8 · Basis: yes

1. `cc-1988` 0.580
2. `cc-2003` 0.574
3. `cc-2030` 0.573
4. `cc-1987` 0.571 ✓
5. `cc-1986` 0.564

Answer (grounded: yes, cited: cc-1988): Sí, tienes derecho a una indemnización por el daño que causaste al entrar sin permiso y molestarte en tu apartamento.

### q033: Se cae el taco de la luz por una falla de la instalación eléctrica vieja, ¿quién la arregla?

Expected: cc-1985, cc-2028, d1077-2.2.6.1.1.10 · Basis: yes

1. `cc-2028` 0.439 ✓
2. `cc-1997` 0.426
3. `cc-1998` 0.426
4. `cc-1988` 0.419
5. `cc-2033` 0.415

Answer (grounded: yes, cited: cc-1998): El arrendatario es responsable de las reparaciones locativas, lo que incluye reparaciones causadas por fallas de instalaciones eléctricas, siempre que no sean por su culpa. Si la falla es por la mala calidad del edificio debido a su vetustez, el arrendatario no será responsable.

### q034: ¿Qué pasa si me atraso un periodo entero en el pago de la renta según el Código Civil?

Expected: l820-22 · Basis: yes

1. `cc-2013` 0.621
2. `cc-2003` 0.619
3. `cc-2002` 0.606
4. `cc-2034` 0.600
5. `cc-1984` 0.578

Answer (grounded: yes, cited: cc-2013): Si me atraso un periodo entero en el pago de la renta, el arrendatario será obligado a pagar la renta de todos los días que falten para que cese, aunque voluntariamente restituya la cosa antes del último día, según el artículo 2013 del Código Civil.

### q036: ¿Cómo se determina la cuantía de los procesos de tenencia por arrendamiento?

Expected: reject · Basis: yes

1. `l820-18` 0.641
2. `l820-5` 0.621
3. `cc-2027` 0.601
4. `l820-4#1` 0.601
5. `cc-2002` 0.600

Answer (grounded: yes, cited: l820-18): La cuantía de los procesos de tenencia por arrendamiento se determina por las partes en un porcentaje del valor comercial del inmueble o de la parte que se dé en arriendo, no excediendo el uno por ciento (1%) del valor comercial.

### q040: ¿Cuál es el salario mínimo en Colombia?

Expected: reject · Basis: yes

1. `l820-19` 0.468
2. `l820-1` 0.401
3. `l820-18` 0.378
4. `l820-34` 0.375
5. `l820-27` 0.366

Answer (grounded: yes, cited: l820-19): El salario mínimo en Colombia es de 1.400.000 COP (Colombianos Pesos) por mes.

