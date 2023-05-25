
const path = require('path');
const ESLintPlugin = require('eslint-webpack-plugin');
const TerserPlugin = require('terser-webpack-plugin');
const { CleanWebpackPlugin } = require("clean-webpack-plugin");

const pkJson = require('./package.json');


const configs = {
  entry: './src/main.ts',
  output: {
    path: path.resolve(__dirname, './dist'),
    publicPath: '/dist/',
    filename: 'main.js',

    library: {
      name: pkJson.name,
      type: 'umd',
      umdNamedDefine: true,
    },
    globalObject: 'this',
  },
  module: {
    rules: [
      {
        test: /\.tsx?$/,
        use: 'ts-loader',
        exclude: /node_modules/,
      },
      { test: /\.js$/, exclude: /node_modules/, loader: 'babel-loader' },
      {
        test: /\.s[ac]ss$/i,
        exclude: /node_modules/,
        type: 'asset/resource',
        generator: {
          filename: '../../Resources/Styles.css',
        },
        use: [
          {
            loader: 'sass-loader',
            options: {
              api: 'modern',
              sourceMap: true,
              sassOptions: {
                outputStyle: 'compressed',
              },
            },
          },
        ],
      }
    ],
  },
  resolve: {
    extensions: ['.ts', '.js', '.scss', '.css'],
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  plugins: [
    new ESLintPlugin({
      cache: true,
      eslintPath: require.resolve('eslint'),
      resolvePluginsRelativeTo: __dirname,
      ignore: true,
      useEslintrc: true,
      extensions: ['ts', 'js'],
    }),
    new CleanWebpackPlugin({
      cleanStaleWebpackAssets: false,
      cleanOnceBeforeBuildPatterns: [path.resolve(__dirname, './dist')],
    }),
  ],
};


module.exports = (env, argv) => {
  const isProduction = argv.mode === 'production';

  return {
    ...configs,
    devtool: 'source-map',
    optimization: {
      minimize: isProduction,
      minimizer: [
        new TerserPlugin({
          terserOptions: {
            sourceMap: true,
            compress: isProduction,
          },
        }),
      ],
    },
  };
}