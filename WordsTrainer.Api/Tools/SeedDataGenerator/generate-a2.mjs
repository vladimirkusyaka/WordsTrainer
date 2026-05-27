import { mkdir, writeFile } from "node:fs/promises";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import rows from "./a2-core.mjs";
import additionalRows from "./a2-additional.mjs";
import finalRows from "./a2-final.mjs";

rows.push(...additionalRows);
rows.push(...finalRows);

const normalizedRows = rows.map((row) => {
  if (row.length === 10) {
    const inferredEnglishWord = row[0].split("_")[0].replaceAll("-", " ");
    return [...row.slice(0, 4), inferredEnglishWord, ...row.slice(4)];
  }

  if (row.length !== 11) {
    throw new Error(`${row[0]}: expected 11 fields, received ${row.length}`);
  }

  return row;
});

const concepts = normalizedRows.map(([
  key, partOfSpeech, de, ru, en,
  ruMeaning, enMeaning, deMeaning,
  deExample, ruExample, enExample
]) => ({
  key,
  level: "A2",
  partOfSpeech,
  description: enMeaning,
  words: [
    { languageCode: "de", text: de },
    { languageCode: "ru", text: ru },
    { languageCode: "en", text: en }
  ],
  explanations: [
    { languageCode: "ru", text: `${ruMeaning} Пример: «${deExample}» - «${ruExample}»` },
    { languageCode: "en", text: `${enMeaning} Example: "${deExample}" - "${enExample}"` },
    { languageCode: "de", text: `${deMeaning} Beispiel: "${deExample}"` }
  ]
}));

const keys = new Set();
for (const concept of concepts) {
  if (!concept.key || keys.has(concept.key)) {
    throw new Error(`Missing or duplicate A2 key: ${concept.key}`);
  }
  keys.add(concept.key);

  for (const languageCode of ["de", "ru", "en"]) {
    const word = concept.words.find((item) => item.languageCode === languageCode)?.text;
    const explanation = concept.explanations.find((item) => item.languageCode === languageCode)?.text;
    if (!word?.trim() || !explanation?.trim()) {
      throw new Error(`${concept.key}: missing ${languageCode} content`);
    }
    if (explanation.length > 2000) {
      throw new Error(`${concept.key}: ${languageCode} explanation exceeds database limit`);
    }
  }
}

const outDir = join(dirname(fileURLToPath(import.meta.url)), "SeedData");
await mkdir(outDir, { recursive: true });
await writeFile(join(outDir, "a2.json"), `${JSON.stringify(concepts, null, 2)}\n`, "utf8");
console.log(`Generated ${concepts.length} A2 concepts in ${join(outDir, "a2.json")}`);
