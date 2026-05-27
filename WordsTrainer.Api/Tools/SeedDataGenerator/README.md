# Seed Data Generator

Source vocabulary data and generation scripts for the WordsTrainer CEFR seed set.

Generated application seed files are stored in:

`WordsTrainer.Api/SeedData`

The generator sources cover:

- A1: 318 concepts
- A2: 300 concepts
- B1: 301 concepts
- B2: 300 concepts
- C1: 300 concepts
- C2: 300 concepts

Total: 1819 concepts.

Each concept includes German, Russian, and English words plus localized explanations and usage examples.

Validation performed before delivery:

- unique concept keys;
- required `de`, `ru`, and `en` content;
- no duplicate answer forms within the same part of speech;
- explanation lengths within database limits.

Generated with AI-assisted engineering using OpenAI Codex and reviewed as application seed content.