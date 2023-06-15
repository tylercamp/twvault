# twvault

This is the original, full source code for my Tribal Wars Vault: https://forum.tribalwars.net/index.php?threads/vault.282252/

The public Vault has been taken down but you can host your own Vault. See [this video](https://www.youtube.com/watch?v=IRPJ-ld-3xQ) for a demonstration on deploying it in under 10 minutes on Linode. You'll need to buy a domain name.

## Development

This code is not being regularly maintained. I encourage others to fork this repository. I may accept pull requests, but it's preferred for others to make a copy and manage PRs independent of this one.

Questions about the project can be submitted as GitHub issues on this repository. Please review existing issues before submitted a new one. Ask the question even if you're unsure whether it'll be answered - if I don't respond, it can still be used for discussion with others.

General discussion can be had on the [Vault discord server](https://discord.gg/7N4UUX8D) or the general [TW Scripts discord server](https://discord.gg/9F5K2rSS).

# Repository Overview

Main folders:

- [`/app`](app) - contains all code for the Vault
- [`/dump`](dump) - two dumps of config files and related data from the Public Vault and the Cicada Vault

(These have READMEs for more details.)

Old folders:

- `/.vscode` - config files for VS code, not extensively used or tested
- `/docker` - an old set of files for generating various Docker images, not used or maintained; these were at least partially functional at some point
- `/k8s/twvault` - an old Helm 2 chart for deploying Vault on k8s; some work was done but this remains largely unfinished
