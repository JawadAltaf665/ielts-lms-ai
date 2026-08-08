import js from '@eslint/js'
import eslintConfigPrettier from 'eslint-config-prettier'
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'
import { defineConfig, globalIgnores } from 'eslint/config'
import globals from 'globals'
import tseslint from 'typescript-eslint'

export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [js.configs.recommended, tseslint.configs.recommended, eslintConfigPrettier],
    plugins: {
      'react-hooks': reactHooks,
      'react-refresh': reactRefresh,
    },
    languageOptions: {
      ecmaVersion: 2023,
      globals: globals.browser,
    },
    rules: {
      ...reactHooks.configs.recommended.rules,
      'react-refresh/only-export-components': 'warn',
      // features/* may import shared/*; shared/* must never import features/*
      'no-restricted-imports': [
        'warn',
        {
          patterns: [
            {
              group: ['*/features/*/*', '!*/features/*/index'],
              message: "Import from a feature's public index.ts, not its internals.",
            },
          ],
        },
      ],
    },
  },
])
