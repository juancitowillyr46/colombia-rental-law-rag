# Corpus sources

**Corpus cutoff date:** 2026-11-19 (validity notes checked against the sources below on that date).

| Law | File | Scope | Primary source | Cross-checked against | Downloaded |
|---|---|---|---|---|---|
| Código Civil | `codigo-civil-arrendamiento.md` | Title XXVI, chapters I–V: arts. 1973–2035 (63 articles, 1 repealed) | Secretaría del Senado, [pr061](http://www.secretariasenado.gov.co/senado/basedoc/codigo_civil_pr061.html), [pr062](http://www.secretariasenado.gov.co/senado/basedoc/codigo_civil_pr062.html), [pr063](http://www.secretariasenado.gov.co/senado/basedoc/codigo_civil_pr063.html) (updated 2026-11-15) | [Normograma Cancillería](https://cancilleria.gov.co/sites/default/files/Normograma/docs/codigo_civil_pr062.htm) (updated 2019-07-31) | 2026-11-19 |
| Ley 820 de 2003 | `ley-820-2003.md` | Complete: arts. 1–43 (43 articles, 5 repealed) | Secretaría del Senado, [ley_0820_2003](http://www.secretariasenado.gov.co/senado/basedoc/ley_0820_2003.html) and [pr001](http://www.secretariasenado.gov.co/senado/basedoc/ley_0820_2003_pr001.html) | [Gestor Normativo, Función Pública](https://www.funcionpublica.gov.co/eva/gestornormativo/norma.php?i=8738) | 2026-11-19 |
| Decreto 1077 de 2015 | `decreto-1077-2015-reparaciones.md` | Art. 2.2.6.1.1.10 only (definition of *reparaciones locativas*) | [Gestor Normativo, Función Pública](https://www.funcionpublica.gov.co/eva/gestornormativo/norma.php?i=77216) | — | 2026-11-19 |

## Out of scope

- Código Civil arts. 2036 onwards (rural leases, leases of works and services).
- The rest of Decreto 1077 de 2015.

## How the text was prepared

- Only the text of the law is kept. Editorial epigraphs (`<...>` titles), validity notes, concordances and editor notes from the portals are **not** reproduced: they are copyrighted value-added content of the compilers. Official article titles that are part of the law itself (e.g. Ley 820 "OBJETO.") are kept.
- Validity was taken from each article's validity notes in the primary source:
  - Repealed (`<!-- status: repealed -->`): CC 2035 (Ley 820 art. 43); Ley 820 arts. 35, 36, 38, 39 and 40 (Ley 1564 de 2012 art. 626 lit. c).
  - Parts declared INEXEQUIBLE are omitted (Ley 820 arts. 12 and 37); replaced text uses the current version (Ley 820 art. 41).
- Source typos fixed after cross-checking: CC 2033, Ley 820 arts. 2 and 34.
- Every change from the source is recorded in an `<!-- note: ... -->` comment under the article heading.
- The raw downloads live in `data/raw/`, which is git-ignored because it contains the portals' copyrighted notes.
