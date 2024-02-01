import React from 'react';
import clsx from 'clsx';
import styles from './styles.module.css';
import { useHistory, useLocation } from '@docusaurus/router';

const FeatureList = [
  {
    title: 'Responsive Design',
    Svg: require('@site/static/svg/waterfall.svg').default,
    description: (
      <>
        Focus on what matters and get things done quicker with our low-profile
        layout, designed to minimize the steps between A and B so you stay flowing.
      </>
    ),
  },
  {
    title: 'Integrated Experience',
    Svg: require('@site/static/svg/yin-yang.svg').default,
    description: (
      <>
        MonkeyPaste was designed from the ground up to <b>evolve your clipboard </b>
        into a vault for your bookmarks, notes and much more.
      </>
    ),
  },
  {
    title: 'Extensible Environment',
    Svg: require('@site/static/svg/seedling.svg').default,
    description: (
      <>
        Built to grow using a simple plugin system that allows developers
        to <a href="./docs/plugins/plugin-development">easily extend</a> and users to pick the features they want with point-and-click ease.
      </>
    ),
  },
];

function Feature({ Svg, title, description }) {
  return (
    <div className={clsx('col col--4')}>
      <div className="text--center">
        <Svg className={styles.featureSvg} role="img" />
      </div>
      <div className="text--center padding-horiz--md">
        <h3>{title}</h3>
        <p>{description}</p>
      </div>
    </div>
  );
}
const ExampleList = [
  {
    imgSrc: require('@site/static/img/ss/win/ss1.png').default,
    description: (
      <>
        ● <a href="./docs/triggers">Trigger & Action designer</a> for custom automations and action chaining
      </>
    ),
  },
  {
    imgSrc: require('@site/static/img/ss/win/ss5.png').default,
    description: (
      <>
        ● Ever-growing collection of <a href="https://github.com/monkeypaste/ledger">community-driven plugins</a><br />
      </>
    ),
  },
  {
    imgSrc: require('@site/static/img/ss/win/ss3.png').default,
    description: (
      <>
        ● Works with rich text (tables, lists, links, etc.)<br />
        ● Fully-featured clip editor with find/replace and highlighting<br />
        ● Store your images in the secure database, ready to use as files anytime on-demand just drag-and-drop!<br />
        ● Powerful <a href="./docs/templates">text templating</a> for quick, dynamic pasting from your snippet collection<br />
      </>
    ),
  },
  {
    imgSrc: require('@site/static/img/ss/win/ss2.png').default,
    description: (
      <>
        ● Simple and friendly interface<br />
        ● Horizontal/vertical layouts, list/grid view and multi-monitor support<br />
        ● Both light & dark themes are completely dynamic<br />
      </>
    ),
  },
  {
    imgSrc: require('@site/static/img/ss/win/ss4.png').default,
    description: (
      <>
        ● Optional 2-factor password protection<br />
      </>
    ),
  },
];
function Example({ imgSrc, description }) {
  return (
    <div class="examples">
      <div className={clsx('row row--6')}>
        <div class="text--center padding-horiz--md">
          <img src={imgSrc} className={styles.exampleImg} role="img" />
        </div>
        <div class="text--left padding-horiz--md">
          <p>{description}</p>
        </div>
      </div>
      <hr />
    </div>
  );
}

export default function HomepageFeatures() {
  return (
    <div>
      <section className={styles.features}>
        <div className="container">
          <div className="row">
            {FeatureList.map((props, idx) => (
              <Feature key={idx} {...props} />
            ))}
          </div>
        </div>
      </section>
      <p align="center">
        <img src={require('/img/ss/logo_and_slogan.png').default} width="500" />
      </p>

      <p align="center">
        <video id="teaserVid" controls height="300">
          <source src={require('/videos/teaser.mp4').default} />
        </video>
      </p>

      <section className={styles.examples}>
        <div className="container">
          <div className="row">
            {ExampleList.map((props, idx) => (
              <Example key={idx} {...props} />
            ))}
          </div><br />
        </div>
      </section>
    </div>
  );
}

