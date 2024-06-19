module.exports = {
    arrowParens: 'always',
    bracketSameLine: false,
    endOfLine: 'lf',
    printWidth: 90,
    singleQuote: true,
    jsxSingleQuote: true,
    bracketSpacing: true,
    semi: true,
    useTabs: false,
    tabWidth: 4,
    trailingComma: 'all',

    overrides: [
        {
            files: 'package.json',
            options: {
                tabWidth: 4,
            },
        },
        {
            files: '*.less',
            options: {
                parser: 'less',
                singleQuote: false,
            },
        },
        {
            files: '*.md',
            options: {
                parser: 'markdown',
                tabWidth: 2,
            },
        },
        {
            // https://github.com/prettier/prettier/issues/15956
            files: '*.json',
            options: {
                trailingComma: 'none',
            },
        },
    ],
};
