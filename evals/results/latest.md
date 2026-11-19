# Retrieval evaluation

Mode: retrieval only · Embedding model: `bge-m3` · Top K: 5 · MinSimilarity: 0.500 · Run: 2026-11-19 23:15 UTC

| Metric | Tuning | Test |
|---|---|---|
| Questions (answerable) | 15 (13) | – |
| Recall@5 | 88 % | – |
| Hit rate@5 | 100 % | – |
| MRR | 0.846 | – |
| Correct rejection | 87 % | – |
| Min top similarity, answerable | 0.488 | – |
| Max top similarity, should reject | 0.591 | – |
| Mean latency (s) | 0.693 | – |

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

