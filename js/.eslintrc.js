module.exports = {
    "parser": "@typescript-eslint/parser",
    "root": true,
    "env": {
        "browser": true,
        "node": true,
        "es2015": true,
        "mocha": true,
        "jest": true
    },
    "extends": [
        "eslint:recommended",
        "plugin:@typescript-eslint/recommended",
        "plugin:import/typescript",
        "plugin:import/recommended",
        "plugin:security/recommended",
        "plugin:prettier/recommended" // Recommended to be last
        // Currently samples contain no frontend
        // "plugin:react/recommended",
        // "plugin:react/jsx-runtime"
    ],
    "plugins": ["@typescript-eslint", "mocha", "only-warn", "prettier"],
    "parserOptions": {
        "ecmaVersion": 2015,
        // Allows for the parsing of modern ECMAScript features
        "sourceType": "module" // Allows for the use of imports
        // "ecmaFeatures": {
        //     "jsx": true
        // }
    },
    "rules": {
        // Place to specify ESLint rules. Can be used to overwrite rules specified from the extended configs
        "@typescript-eslint/ban-types": "off",
        "@typescript-eslint/explicit-function-return-type": "off",
        "@typescript-eslint/explicit-member-accessibility": "off",
        "@typescript-eslint/explicit-module-boundary-types": "off",
        "@typescript-eslint/interface-name-prefix": "off",
        "@typescript-eslint/no-empty-function": "off",
        "@typescript-eslint/no-explicit-any": "off",
        "@typescript-eslint/no-namespace": "off",
        "@typescript-eslint/no-non-null-assertion": "off",
        "@typescript-eslint/no-unused-vars": "off",
        "no-async-promise-executor": "off",
        "no-constant-condition": "off",
        "no-undef": "off", // Disabled due to conflicts with @typescript/eslint
        "no-unused-vars": "off", // Disabled due to conflicts with @typescript/eslint
        "prettier/prettier": "error",
        "jsdoc/empty-tags": "off"
        // Currently samples contain no frontend
        // "react/react-in-jsx-scope": "off",
        // "react/prop-types": "off"
    },
    "overrides": [
        {
            "files": ["bin/*.js", "lib/*.js"]
        }
    ],
    "ignorePatterns": ["node_modules/*"],
    "settings": {
        // "react": {
        //     "version": "detect"
        // }
    }
};
