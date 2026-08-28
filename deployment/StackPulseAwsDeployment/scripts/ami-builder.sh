#!/bin/bash
set -euo pipefail
# Dedicated builder workflow:
# 1) install Docker
# 2) copy the compiled Java application/build context
# 3) docker build -t <app>:<version> .
# 4) docker save the image and place it under /opt/stackpulse/images
# 5) install compose/bootstrap files under /opt/stackpulse
# 6) create an EBS-backed AMI from the builder instance
# Never copy Secrets Manager values into the AMI.
