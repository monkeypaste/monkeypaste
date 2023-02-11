const path = require("path");
var webpack = require("webpack");
const HtmlWebpackPlugin = require("html-webpack-plugin");

module.exports = {
  mode: "development",
  entry: "./src/app.js",
  plugins: [
    new HtmlWebpackPlugin({
      hash: true,
      title: "My Awesome application",
      myPageHeader: "Hello World",
      template: "./src/index.html"
    }),
    new webpack.ProvidePlugin({
      $: "jquery",
      jQuery: "jquery",
      "window.jQuery": "jquery",
      "window.$": "jquery"
    })
  ],
  module: {
    rules: [
      {
        test: /\.js$/i,
        use: ["style-loader", "css-loader", "source-map-loader"]
      }
    ]
  },
  output: {
    filename: "[name].bundle.js",
    path: path.resolve(__dirname, "dist"),
    sourceMapFilename: "[name].js.map",
    clean: true
    },
    devtool: "source-map"
};
