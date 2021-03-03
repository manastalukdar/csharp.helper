# CI, CD

1. [Current Status](#current-status)
2. [Methodology](#methodology)
    1. [Committing code](#committing-code)
    2. [Versioning of packages/deliverables](#versioning-of-packagesdeliverables)
    3. [Builds](#builds)
    4. [Deployments](#deployments)
    5. [Status badges](#status-badges)

## Current Status

| Platform       | Provider        | Operations                              | Status                                                                                                                                                                                                                                                                                 |
| -------------- | --------------- | --------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Linux          | GitHub          | Build, Test, Deploy to gh-pages         | [![build-test-deploy](https://github.com/manastalukdar/helpsharp/workflows/build-test-deploy/badge.svg)](https://github.com/manastalukdar/helpsharp/actions)                                                                                                                   |
| Linux          | GitHub          | Linter                                  | [![Lint Code Base](https://github.com/manastalukdar/helpsharp/actions/workflows/linter.yml/badge.svg)](https://github.com/manastalukdar/helpsharp/actions/workflows/linter.yml)                                                                                                |
| Windows, Linux | AppVeyor        | Build, Test                             | [![Build status](https://ci.appveyor.com/api/projects/status/y9qup8owf4e53m8h?svg=true)](https://ci.appveyor.com/project/manastalukdar/helpsharp)                                                                                                                                  |
| Linux          | Travis CI       | Build, Test, Deploy to gh-pages         | [![Build Status](https://travis-ci.org/manastalukdar/helpsharp.svg?branch=main)](https://travis-ci.com/manastalukdar/helpsharp)                                                                                                                                                |
| Linux          | Circle CI       | Build, Test                             | [![CircleCI](https://circleci.com/gh/manastalukdar/helpsharp/tree/main.svg?style=svg)](https://circleci.com/gh/manastalukdar/helpsharp/tree/main)                                                                                                                              |
| Linux          | Azure Pipelines | Build, Test                             |                                                                                                                                                                                                                                                                                        |
| N/A            | CodeClimate     | Check for code maintainability          | [![Maintainability](https://api.codeclimate.com/v1/badges/2e2bfc548d29f566051a/maintainability)](https://codeclimate.com/github/manastalukdar/helpsharp/maintainability)                                                                                                           |
| N/A            | CodeClimate     | Check for test coverage using Circle CI | [![Test Coverage](https://api.codeclimate.com/v1/badges/2e2bfc548d29f566051a/test_coverage)](https://codeclimate.com/github/manastalukdar/helpsharp/test_coverage)                                                                                                                 |
| N/A            | Codacy          | Code analysis/quality check             | [![Codacy Badge](https://app.codacy.com/project/badge/Grade/b10299a64e704411ba321229fcad3e04)](https://www.codacy.com/gh/manastalukdar/helpsharp/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=manastalukdar/helpsharp&amp;utm_campaign=Badge_Grade) |
| N/A            | CodeBeat        | Static code analysis                    | [![codebeat badge](https://codebeat.co/badges/29ea0010-206a-433c-813d-55eba8e9edaf)](https://codebeat.co/projects/github-com-computer-science-engineering-learning-computer-science-main)                                                                                              |
| N/A            | CodeFactor      | Static code analysis                    | [![CodeFactor](https://www.codefactor.io/repository/github/manastalukdar/helpsharp/badge)](https://www.codefactor.io/repository/github/manastalukdar/helpsharp)                                                                                                                |
| N/A            | Codecov         | Code coverage using Circle CI           | [![codecov](https://codecov.io/gh/manastalukdar/helpsharp/branch/main/graph/badge.svg?token=IN47ioiCTU)](undefined)                                                                                                                                                                |
| N/A            | Coveralls       | Code coverage using Circle CI           | [![Coverage Status](https://coveralls.io/repos/github/manastalukdar/helpsharp/badge.svg?branch=main)](https://coveralls.io/github/manastalukdar/helpsharp?branch=main)                                                                                                         |

## Methodology

Implement CI-CD pipelines with the following functionality. Consider using GitHub actions.

### Committing code

- Code should be pushed into `main` only by using PRs. Disable direct push functionality into `main`.
- Disable PRs into `main` lacking reference to any issue.
- Disable commits and PRs into `develop` lacking reference to any issue.

### Versioning of packages/deliverables

Versioning of packages published by different branches:

- `main` deploys stable production code. `latest` tag should be used for the corresponding package.
- `develop` deploys under-development code. `next` tag should be used for the corresponding package.

### Builds

Automated builds should be triggered on:

- Pushing code to `develop`.
- Creating new PR into `develop`.
- Creating new PR into `main`.

### Deployments

Automated deployments should be triggered on:

- Successful build triggered by direct push of code into `develop`.
- Successful merge of a PR into `develop`.
- Successful merge of a PR into `main`.

Deployment pipeline should:

1. Tag the branch with the correct semantic version number. Investigate using some open-source automated semver tooling.
2. Create GitHub release. The Release text can be initially (and automatically) populated by issues (title and link) referenced by the direct push commit or the PR that just got merged in.
3. Publish package to relevant package registry.
    - On `develop` package should be tagged as `next`.
    - On `main` package should be tagged as `latest`.
    - **Question**: What about release candidates? `rc` tags? Which branch does this go from? Typically this goes from an intermediate release branch. How would this tie into the automation?

It is worth considering if steps 1, 2 and 3 above:

- should be automated by a deployment pipeline, or
- should be left as manual, or
- should be automated but manual deployments should also be allowed.

### Status badges

CI/CD status badges should be provided in the README.md file for builds and deployments from both `develop` and `main` branches.
