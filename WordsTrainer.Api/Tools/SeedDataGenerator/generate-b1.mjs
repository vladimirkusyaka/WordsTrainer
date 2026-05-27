import { mkdir, writeFile } from "node:fs/promises";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import coreRows from "./b1-core.mjs";
import additionalRows from "./b1-additional.mjs";
import finalRows from "./b1-final.mjs";
import completionRows from "./b1-completion.mjs";

const rows = [...coreRows, ...additionalRows, ...finalRows, ...completionRows];

for (const row of rows) {
  if (row.length !== 11) {
    throw new Error(`${row[0]}: expected 11 fields, received ${row.length}`);
  }
}

const concepts = rows.map(([
  key, partOfSpeech, de, ru, en,
  ruMeaning, enMeaning, deMeaning,
  deExample, ruExample, enExample
]) => ({
  key,
  level: "B1",
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
    throw new Error(`Missing or duplicate B1 key: ${concept.key}`);
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
await writeFile(join(outDir, "b1.json"), `${JSON.stringify(concepts, null, 2)}\n`, "utf8");
console.log(`Generated ${concepts.length} B1 concepts in ${join(outDir, "b1.json")}`);
