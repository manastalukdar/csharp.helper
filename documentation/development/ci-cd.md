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
| Linux          | GitHub          | Build, Test, Deploy to gh-pages         | [![build-test](https://github.com/manastalukdar/helpsharp/actions/workflows/main.yml/badge.svg)](https://github.com/manastalukdar/helpsharp/actions/workflows/main.yml)                                                                                                                   |
| Linux          | GitHub          | Linter                                  | [![Lint Code Base](https://github.com/manastalukdar/helpsharp/actions/workflows/linter.yml/badge.svg)](https://github.com/manastalukdar/helpsharp/actions/workflows/linter.yml)

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
