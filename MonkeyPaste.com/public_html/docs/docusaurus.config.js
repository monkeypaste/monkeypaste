// @ts-check
// Note: type annotations allow type checking and IDEs autocompletion
require('dotenv').config();

const { themes } = require('prism-react-renderer');
const lightTheme = themes.synthwave84;
const darkTheme = themes.dracula;

const local = true;//process.env.NODE_ENV == 'local';

const siteUrl = local ?
  "https://localhost" :
  "https://monkeypaste.com";

const baseUrl = local ?
  "/docs/build" :
  "/";

var aboutUrl = "/blog";
var downloadUrl = "/blog";

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: 'MonkeyPaste',
  staticDirectories: ['static'],
  tagline: '(coming soon!)',
  favicon: 'img/favicon.ico',
  url: siteUrl,
  baseUrl: baseUrl,

  // GitHub pages deployment config.
  // If you aren't using GitHub pages, you don't need these.
  organizationName: 'Monkey LLC', // Usually your GitHub org/user name.
  projectName: 'MonkeyPaste', // Usually your repo name.

  onBrokenLinks: 'throw',
  onBrokenMarkdownLinks: 'warn',

  // Even if you don't use internalization, you can use this field to set useful
  // metadata like html lang. For example, if your site is Chinese, you may want
  // to replace "en" with "zh-Hans".
  i18n: {
    defaultLocale: 'en',
    locales: ['en'],
  },

  presets: [
    [
      'classic',
      /** @type {import('@docusaurus/preset-classic').Options} */
      ({
        docs: {
          sidebarPath: require.resolve('./sidebars.js'),
          editUrl: 'https://github.com/monkeypaste/monkeypaste-docs/tree/main/'
        },
        blog: {
          showReadingTime: true,
          // Please change this to your repo.
          // Remove this to remove the "edit this page" links.
          //editUrl:
          //  'https://github.com/facebook/docusaurus/tree/main/packages/create-docusaurus/templates/shared/',
        },
        pages: {
          path: 'src/pages',
          routeBasePath: '',
          include: ['**/*.{js,jsx,ts,tsx,md,mdx}'],
          exclude: [
            '**/_*.{js,jsx,ts,tsx,md,mdx}',
            '**/_*/**',
            '**/*.test.{js,jsx,ts,tsx}',
            '**/__tests__/**',
          ],
          mdxPageComponent: '@theme/MDXPage',
          // remarkPlugins: [require('./my-remark-plugin')],
          rehypePlugins: [],
          beforeDefaultRemarkPlugins: [],
          beforeDefaultRehypePlugins: [],
        },
        theme: {
          customCss:
            [
              require.resolve('./src/css/content-styles.css'),
              require.resolve('./src/css/custom.css'),
              require.resolve('./src/css/help-style.css'),
              require.resolve('./src/css/app-update-style.css'),
            ]
        },
      }),
    ],
  ],

  themeConfig:
    /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
    ({
      zoom: {
        selector: '.markdown img',
        background: {
          light: 'rgb(255, 255, 255)',
          dark: 'rgb(50, 50, 50)'
        },
        config: {
          // options you can specify via https://github.com/francoischalifour/medium-zoom#usage
        }
      },
      image: 'img/monkeypaste-social-card.jpg',
      prism: {
        theme: lightTheme,
        darkTheme: darkTheme,
        additionalLanguages: ['csharp'],
      },
      navbar: {
        title: 'MonkeyPaste',
        logo: {
          alt: 'MonkeyPaste',
          src: 'img/logo.svg',
        },
        items: [
          {
            type: 'docSidebar',
            sidebarId: 'tutorialSidebar',
            position: 'left',
            label: 'Docs',
          },
          { to: '/blog', label: 'Blog', position: 'left' },
          { to: aboutUrl, label: 'About', position: 'left' },
          { to: 'https://www.monkeypaste.com/forum', label: 'Forum', position: 'right' },
          { to: downloadUrl, label: 'Download', position: 'right' },
          {
            href: 'https://github.com/monkeypaste',
            label: 'GitHub',
            position: 'right',
          },
        ],
      },
      footer: {
        style: 'dark',
        links: [
          {
            title: 'Docs',
            items: [
              {
                label: 'Welcome',
                to: '/docs/welcome',
              },
            ],
          },
          {
            title: 'Community',
            items: [
              {
                label: 'Stack Overflow',
                href: 'https://stackoverflow.com/questions/tagged/monkeypaste',
              },
              {
                label: 'Discord',
                href: 'https://discordapp.com/invite/monkeypaste',
              },
              {
                label: 'Twitter',
                href: 'https://twitter.com/monkeypaste',
              },
            ],
          },
          {
            title: 'More',
            items: [
              {
                label: 'Blog',
                to: '/blog',
              },
              {
                label: 'GitHub',
                href: 'https://github.com/monkeypaste',
              },
            ],
          },
        ],
        copyright: `Copyright © ${new Date().getFullYear()} Monkey LLC, Built with Docusaurus.`,
      },
    }),
  markdown: {
    mermaid: true,
  },
  themes: ['@docusaurus/theme-mermaid'],
  plugins: [
    'image-zoom',
    [
      require.resolve("@cmfcmf/docusaurus-search-local"),
      {
        // Options here
      },
    ],
  ],
};

module.exports = config;
