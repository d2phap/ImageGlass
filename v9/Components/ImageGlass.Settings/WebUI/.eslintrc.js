const isProduction = process.env.NODE_ENV === 'production';

module.exports = {
  root: true,
  env: {
    browser: true,
    es2021: true,
  },
  extends: [
    'airbnb-typescript',
  ],
  parser: '@typescript-eslint/parser',
  parserOptions: {
    ecmaVersion: 12,
    sourceType: 'module',
    tsconfigRootDir: './',
    project: 'tsconfig.json',
  },
  plugins: [
    'import',
  ],
  settings: {
    // To kill annoyed React warning:
    // https://github.com/DRD4-7R/eslint-config-7r-building/issues/1#issuecomment-714491844
    react: {
      version: '999.999.999',
    },
  },
  rules: {
    'react/jsx-filename-extension': 'off',
    'no-multiple-empty-lines': ["error", {
      max: 2,
      maxEOF: 1,
      maxBOF: 1,
    }],
    '@typescript-eslint/ban-types': 'off',
    '@typescript-eslint/ban-ts-comment': 'off',
    '@typescript-eslint/brace-style': 'off',
    '@typescript-eslint/lines-between-class-members': 'off',
    '@typescript-eslint/no-explicit-any': 'off',
    '@typescript-eslint/no-unused-vars': isProduction ? 'error' : 'warn',
    'arrow-parens': 'off',
    'brace-style': ['error', 'stroustrup', { allowSingleLine: true }],
    camelcase: 'off',
    'class-methods-use-this': 'off',
    'import/extensions': 'off',
    'import/no-cycle': 'off',
    'import/no-unresolved': 'off',
    'linebreak-style': 'off',
    'lines-between-class-members': 'off',
    'max-classes-per-file': 'off',
    'no-await-in-loop': 'off',
    'no-console': [
      isProduction ? 'error' : 'warn',
      {
        allow: ['info', 'warn', 'error'],
      },
    ],
    'no-continue': 'off',
    'no-debugger': isProduction ? 'error' : 'warn',
    'no-empty': 'off',
    'no-param-reassign': 'off',
    'no-plusplus': 'off',
    'no-restricted-globals': 'off',
    // https://github.com/typescript-eslint/typescript-eslint/blob/main/docs/linting/TROUBLESHOOTING.md#i-get-errors-from-the-no-undef-rule-about-global-variables-not-being-defined-even-though-there-are-no-typescript-errors
    'no-undef': 'off',
    'prefer-object-spread': 'off',
  },
};
