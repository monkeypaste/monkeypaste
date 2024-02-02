import React, { Component } from "react";
import clsx from 'clsx';
import styles from './styles.module.css';
import { useHistory, useLocation } from '@docusaurus/router';


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

export default function HomepageCarousel() {
  return (
    <section className={styles.examples}>
      <p align="center">
        <video id="teaserVid" controls height="300">
          <source src={require('/videos/teaser.mp4').default} />
        </video>
      </p>
      <div className="container">
        <div className="row">
          {ExampleList.map((props, idx) => (
            <Example key={idx} {...props} />
          ))}
        </div><br />
      </div>
    </section>
  );
}

