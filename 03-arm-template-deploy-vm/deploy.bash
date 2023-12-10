#!/bin/bash
#Azure CLI template deploymentaz group create --name AZ204-ResourceGroup --location "West US"az group deployment create \
--name AZ204DemoDeployment \--resource-group AZ204-ResourceGroup \--template-file az204-template.json \--parameters @az204-parameters.