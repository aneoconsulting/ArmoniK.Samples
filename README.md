# ArmoniK.Samples

## Introduction

Code samples showcasing how to use ArmoniK for the different programming languages supported by the framework.

## Software prerequisites

Before you start using the Samples, please follow the Armonik installation that you can find
in [Armonik's official documentation](https://armonik.readthedocs.io/en/latest/)

## Documentation

Documentation can be found at each sample dedicated subdirectory, for an overview please check [here](https://armoniksamples.readthedocs.io/en/latest/)

## Published images

The release workflow publishes the sample images to Docker Hub under `dockerhubaneo/armonik_demo_*`. Before pushing, it scans each image with [Trivy](https://trivy.dev) and stops the release on a fixable `HIGH` or `CRITICAL` vulnerability. Accepted findings are listed, with their reason, in [`.trivyignore`](.trivyignore).

Each published image is signed with [cosign](https://docs.sigstore.dev/cosign/) keyless signing and carries an SPDX SBOM. Images built with Docker also carry a SLSA build provenance.

Verify the signature of an image:

```bash
cosign verify dockerhubaneo/armonik_demo_helloworld_worker:<version> \
  --certificate-identity-regexp '^https://github.com/aneoconsulting/ArmoniK.Samples/.github/workflows/release.yml@refs/tags/v' \
  --certificate-oidc-issuer https://token.actions.githubusercontent.com
```

Read the SBOM and the provenance of an image built with Docker:

```bash
docker buildx imagetools inspect dockerhubaneo/armonik_demo_helloworld_worker:<version> --format '{{ json .SBOM }}'
docker buildx imagetools inspect dockerhubaneo/armonik_demo_helloworld_worker:<version> --format '{{ json .Provenance }}'
```

The SBOM of a Java image (`armonik_demo_java_*`) is a signed cosign attestation:

```bash
cosign verify-attestation --type spdxjson dockerhubaneo/armonik_demo_java_hello-world-worker:<version> \
  --certificate-identity-regexp '^https://github.com/aneoconsulting/ArmoniK.Samples/.github/workflows/release.yml@refs/tags/v' \
  --certificate-oidc-issuer https://token.actions.githubusercontent.com
```
