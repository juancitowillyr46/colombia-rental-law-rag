# Evaluation

Mode: full (retrieval + answer) · Embedding model: `bge-m3` · Chat model: `qwen2.5:7b` · Top K: 5 · MinSimilarity: 0.420 · Run: 2026-11-20 04:35 UTC

| Metric | Tuning | Test |
|---|---|---|
| Questions (answerable) | 40 (32) | 20 (14) |
| Recall@5 | 80 % | 83 % |
| Hit rate@5 | 94 % | 93 % |
| MRR | 0.721 | 0.655 |
| Correct rejection (threshold only) | 80 % | 70 % |
| Correct rejection (end to end) | 95 % | 80 % |
| Citation precision | 77 % | 53 % |
| Citations discarded by validator | 0 | 0 |
| Min top similarity, answerable | 0.439 | 0.440 |
| Max top similarity, should reject | 0.641 | 0.583 |
| Mean latency (s) | 35.585 | 64.245 |

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
| q001 | tuning | clear | cc-1985, cc-2028 | 1 | 0.488 | yes | cc-2028 | 69.335 | ok |
| q002 | tuning | everyday_words | cc-1985, cc-2028 | 1 | 0.587 | yes | cc-2028 | 40.198 | ok |
| q003 | tuning | everyday_words | cc-2029, cc-1998 | 2 | 0.563 | yes | cc-2029 | 36.922 | ok |
| q004 | tuning | everyday_words | cc-2029 | 2 | 0.567 | yes | cc-2029, cc-1998 | 39.757 | ok |
| q005 | tuning | clear | cc-1998, d1077-2.2.6.1.1.10 | 1 | 0.733 | yes | d1077-2.2.6.1.1.10 | 32.241 | ok |
| q006 | tuning | clear | l820-27, cc-1993 | 1 | 0.678 | yes | l820-27 | 22.971 | ok |
| q007 | tuning | everyday_words | l820-20 | 1 | 0.686 | yes | l820-20 | 20.798 | ok |
| q008 | tuning | clear | l820-16 | 1 | 0.749 | yes | l820-16 | 20.507 | ok |
| q009 | tuning | everyday_words | l820-17, cc-2004 | 1 | 0.655 | yes | l820-17 | 19.652 | ok |
| q010 | tuning | everyday_words | l820-24, l820-25 | 1 | 0.653 | yes | l820-24 | 24.914 | ok |
| q011 | tuning | clear | l820-8, cc-1982 | 2 | 0.730 | yes | l820-8, l820-9, cc-1982 | 42.889 | ok |
| q012 | tuning | everyday_words | cc-2028, cc-1985 | 1 | 0.551 | yes | cc-2028 | 41.562 | ok |
| q013 | tuning | repealed | l820-22 | 2 | 0.693 | yes | l820-22 | 71.522 | ok |
| q014 | tuning | out_of_corpus | (reject) | – | 0.591 | no | – | 69.774 | ok |
| q015 | tuning | out_of_corpus | (reject) | – | 0.500 | no | – | 58.733 | ok |
| q016 | tuning | everyday_words | cc-2029, cc-1998 | 3 | 0.483 | yes | cc-2028 | 13.335 | FAIL |
| q017 | tuning | everyday_words | cc-1985, cc-2028 | 2 | 0.471 | yes | cc-2028 | 50.186 | ok |
| q018 | tuning | everyday_words | cc-2028, l820-9 | – | 0.536 | yes | cc-2029 | 26.604 | FAIL |
| q019 | tuning | clear | l820-9 | 2 | 0.771 | yes | l820-9 | 25.356 | ok |
| q020 | tuning | clear | cc-1990, l820-24 | 2 | 0.631 | yes | cc-1990 | 50.270 | ok |
| q021 | tuning | everyday_words | cc-1993, l820-27 | 1 | 0.611 | yes | cc-1993 | 56.341 | ok |
| q022 | tuning | clear | l820-22, cc-2025 | 2 | 0.627 | yes | cc-2025 | 62.599 | ok |
| q023 | tuning | clear | cc-2020, cc-2019 | 1 | 0.572 | yes | cc-2019 | 20.417 | ok |
| q024 | tuning | everyday_words | cc-2013, cc-2003, l820-25 | 1 | 0.567 | yes | cc-2013 | 17.864 | ok |
| q025 | tuning | everyday_words | cc-2032 | 1 | 0.633 | yes | cc-2032 | 15.794 | ok |
| q026 | tuning | clear | l820-23, l820-22 | 2 | 0.751 | yes | l820-22, l820-24 | 30.546 | ok |
| q027 | tuning | everyday_words | l820-22, l820-15 | 2 | 0.622 | yes | l820-22 | 16.564 | ok |
| q028 | tuning | clear | l820-13 | 1 | 0.715 | yes | l820-13 | 18.738 | ok |
| q029 | tuning | clear | l820-19 | 2 | 0.697 | yes | l820-19 | 56.030 | ok |
| q030 | tuning | clear | l820-28 | 1 | 0.679 | yes | l820-28 | 17.026 | ok |
| q031 | tuning | everyday_words | cc-1987, l820-8 | 4 | 0.580 | yes | cc-1988 | 12.211 | FAIL |
| q032 | tuning | clear | cc-1997, l820-9 | 1 | 0.686 | yes | l820-9 | 49.548 | ok |
| q033 | tuning | everyday_words | cc-1985, cc-2028, d1077-2.2.6.1.1.10 | 1 | 0.439 | yes | cc-1998, cc-2028 | 49.843 | ok |
| q034 | tuning | repealed | l820-22 | – | 0.621 | yes | cc-2034, cc-1984 | 49.664 | FAIL |
| q035 | tuning | repealed | (reject) | – | 0.542 | no | – | 9.536 | ok |
| q036 | tuning | repealed | (reject) | – | 0.641 | no | – | 50.275 | ok |
| q037 | tuning | out_of_corpus | (reject) | – | 0.476 | no | – | 40.770 | ok |
| q038 | tuning | out_of_corpus | (reject) | – | 0.515 | yes | l820-28 | 17.532 | FAIL |
| q039 | tuning | out_of_corpus | (reject) | – | 0.561 | yes | cc-1973, cc-1974 | 48.216 | FAIL |
| q040 | tuning | out_of_corpus | (reject) | – | 0.468 | no | – | 6.358 | ok |
| t001 | test | everyday_words | cc-2030, cc-1998, l820-9 | 3 | 0.496 | yes | cc-1985, cc-2030 | 37.031 | ok |
| t002 | test | everyday_words | l820-10 | 1 | 0.597 | yes | l820-10 | 81.617 | ok |
| t003 | test | clear | l820-3 | 1 | 0.730 | yes | l820-3 | 77.911 | ok |
| t004 | test | everyday_words | l820-5 | 1 | 0.665 | yes | l820-5 | 61.902 | ok |
| t005 | test | clear | l820-6 | 4 | 0.701 | yes | cc-2014, l820-24 | 58.714 | FAIL |
| t006 | test | everyday_words | l820-11 | 1 | 0.599 | yes | l820-11 | 73.360 | ok |
| t007 | test | clear | l820-18 | 1 | 0.701 | yes | l820-18 | 58.975 | ok |
| t008 | test | everyday_words | l820-7 | 4 | 0.577 | yes | l820-14 | 71.563 | FAIL |
| t009 | test | everyday_words | cc-2029 | 2 | 0.440 | yes | cc-1998 | 47.026 | FAIL |
| t010 | test | everyday_words | cc-1986 | 3 | 0.589 | yes | cc-1986 | 78.016 | ok |
| t011 | test | clear | cc-1994 | 1 | 0.586 | yes | cc-1994 | 69.525 | ok |
| t012 | test | everyday_words | cc-2006, cc-2005 | 2 | 0.564 | yes | l820-24 | 80.097 | FAIL |
| t013 | test | clear | cc-1996 | – | 0.614 | no | – | 71.283 | FAIL |
| t014 | test | everyday_words | cc-1999, cc-2029 | 1 | 0.503 | yes | cc-2029 | 48.417 | ok |
| t015 | test | repealed | (reject) | – | 0.557 | yes | l820-25 | 67.556 | FAIL |
| t016 | test | repealed | (reject) | – | 0.582 | yes | l820-7 | 63.177 | FAIL |
| t017 | test | out_of_corpus | (reject) | – | 0.583 | no | – | 46.771 | ok |
| t018 | test | out_of_corpus | (reject) | – | 0.511 | yes | l820-29 | 58.734 | FAIL |
| t019 | test | out_of_corpus | (reject) | – | 0.474 | no | – | 70.198 | ok |
| t020 | test | out_of_corpus | (reject) | – | 0.490 | no | – | 63.026 | ok |

## Failed questions

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

Answer (grounded: yes, cited: cc-2029): Según el artículo [cc-2029] del Código Civil, el inquilino está obligado a mantener la integridad interior del edificio, pero no se especifica que deba pintarlo. La obligación principal es conservar la integridad de las paredes, techos, pavimentos y cañerías, reponiendo las piedras, ladrillos y tejas que se quiebren o desencajen, así como reponer los cristales quebrados y mantener en servicio las puertas, ventanas y cerraduras.

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

Answer (grounded: yes, cited: cc-2034, cc-1984): Según el artículo 2034 del Código Civil, el desahucio debe darse con anticipación de un período entero. Si el arrendador se atrasa un periodo entero en el pago de la renta, el arrendatario tiene derecho a indemnización de perjuicios.

### q038: ¿Cómo registro una empresa en la Cámara de Comercio?

Expected: reject · Basis: yes

1. `l820-30` 0.515
2. `l820-28` 0.497
3. `l820-29` 0.460
4. `l820-31` 0.447
5. `l820-10#2` 0.434

Answer (grounded: yes, cited: l820-28): Una persona jurídica que desee matricularse como arrendador de viviendas urbanas debe presentar un documento que acredite su existencia y representación legal ante la autoridad administrativa competente, según establece el artículo 28 de la Ley 820 de 2003.

### q039: ¿Qué dice la ley sobre el arriendo de una finca para cultivar?

Expected: reject · Basis: yes

1. `cc-1973` 0.561
2. `cc-1974` 0.549
3. `cc-1996` 0.547
4. `cc-2020` 0.541
5. `cc-2002` 0.535

Answer (grounded: yes, cited: cc-1973, cc-1974): El arrendamiento de una finca para cultivar está permitido y se regula según las disposiciones del Código Civil, específicamente en los artículos 1973 y 1974.

### t005: ¿El contrato de arriendo se renueva solo cuando se vence?

Expected: l820-6 · Basis: yes

1. `cc-2014` 0.701
2. `l820-24#2` 0.627
3. `cc-2008` 0.615
4. `l820-6` 0.614 ✓
5. `l820-5` 0.612

Answer (grounded: yes, cited: cc-2014, l820-24): El contrato de arriendo puede renovarse de manera automática si no se da aviso previo de terminación por parte del arrendatario, pero también puede ser renovado expresamente por ambas partes mediante un acuerdo inequívoco.

### t008: Somos dos inquilinos en el contrato y mi compañero no pagó, ¿me pueden cobrar todo a mí?

Expected: l820-7 · Basis: yes

1. `l820-14` 0.577
2. `l820-15#2` 0.572
3. `cc-2000` 0.568
4. `l820-7` 0.564 ✓
5. `cc-2003` 0.564

Answer (grounded: yes, cited: l820-14): No, según el artículo [l820-14] de la Ley 820 de 2003, las deudas a cargo del arrendatario por concepto de servicios públicos domiciliarios o expensas comunes dejadas de pagar, el arrendador podrá repetir lo pagado contra el arrendatario por la vía ejecutiva mediante la presentación de las facturas, comprobantes o recibos de las correspondientes empresas debidamente canceladas.

### t009: Se soltó la bisagra de la puerta del baño, ¿me toca arreglarla?

Expected: cc-2029 · Basis: yes

1. `cc-2030` 0.440
2. `cc-2029` 0.435 ✓
3. `cc-1998` 0.418
4. `cc-1986` 0.412
5. `cc-2006` 0.412

Answer (grounded: yes, cited: cc-1998): Sí, según el artículo 1998 del Código Civil, el arrendatario es obligado a realizar reparaciones locativas, como la reemplazación de bisagras dañadas.

### t012: Me voy a ir del apartamento, ¿cómo lo tengo que entregar?

Expected: cc-2006, cc-2005 · Basis: yes

1. `l820-24#2` 0.564
2. `cc-2005` 0.555 ✓
3. `l820-22#2` 0.553
4. `l820-23#1` 0.552
5. `l820-25#1` 0.548

Answer (grounded: yes, cited: l820-24): Debes entregar el inmueble en el estado en que te lo entregaron, considerando el deterioro normal del uso. Si no consta el estado original, se asumirá que está en buen estado de servicio. No necesitas invocar una causa específica para terminar el contrato, ya que puedes hacerlo a tu voluntad con un aviso previo de tres meses.

### t013: ¿Puedo usar la casa que arriendo para un negocio aunque el contrato dice vivienda?

Expected: cc-1996 · Basis: yes

1. `l820-2` 0.614
2. `l820-3` 0.587
3. `l820-4#1` 0.570
4. `cc-2012` 0.566
5. `cc-2032` 0.563

Answer (grounded: no, cited: –): No encontré base suficiente en las normas consultadas (Ley 820 de 2003, Código Civil y Decreto 1077 de 2015) para responder esa pregunta. Si tu caso es particular, consulta con un abogado o con la oficina de vivienda de tu municipio.

### t015: ¿Qué medidas cautelares puede pedir el dueño en un proceso de restitución del inmueble?

Expected: reject · Basis: yes

1. `l820-25#2` 0.557
2. `l820-15#3` 0.553
3. `l820-23#2` 0.543
4. `cc-2007` 0.542
5. `l820-24#3` 0.537

Answer (grounded: yes, cited: l820-25): El arrendador puede solicitar la devolución de la indemnización consignada si el arrendatario no entrega el inmueble en la fecha señalada, según lo establecido en los parágrafos 1o y 2o del artículo 25 de la Ley 820 de 2003.

### t016: ¿Las sentencias de restitución de inmueble arrendado se envían a consulta?

Expected: reject · Basis: yes

1. `l820-7` 0.582
2. `cc-2005` 0.582
3. `l820-25#2` 0.581
4. `l820-12` 0.575
5. `l820-23#2` 0.575

Answer (grounded: yes, cited: l820-7): Las sentencias de restitución de inmueble arrendado no se envían a consulta, sino que pueden ser exigidas o cumplidas por todos o cualquiera de los arrendadores a todos o cualquiera de los arrendatarios, o viceversa.

### t018: ¿Qué requisitos pide un banco para darme un crédito hipotecario?

Expected: reject · Basis: yes

1. `l820-16` 0.511
2. `l820-29` 0.508
3. `cc-2020` 0.486
4. `cc-2029` 0.476
5. `l820-2` 0.475

Answer (grounded: yes, cited: l820-29): Los requisitos para obtener una matrícula de arrendador, como el que mencionas en un banco, se encuentran en el artículo 29 de la Ley 820 de 2003. Debes presentar documentos que acrediten tu existencia y representación legal, el modelo de contrato de arrendamiento, y cumplir con las demás requisitos que determine la autoridad competente.

