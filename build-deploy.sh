#!/bin/bash

file="deploy.yaml"

add_file() {
    echo "---" >> $file
    echo "# Source:" $1 >> $file
    cat $1 >> $file
    echo "" >> $file
}

rm -f $file
echo "# Generated by KubeChat/build-deploy.sh" >> $file
add_file "kubechat/templates/namespace.yaml"
add_file "kubechat/templates/server.deployment.yaml"
add_file "kubechat/templates/server.service.yaml"
add_file "kubechat/templates/client.deployment.yaml"
add_file "kubechat/templates/client.service.yaml"
add_file "kubechat/templates/ingress.yaml"

echo "Generated" $file