#!/bin/bash

# Скачайте и запустите официальный скрипт установки
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh

# Установите .NET SDK 10.0
./dotnet-install.sh --channel 10.0

# Добавьте в PATH
echo 'export DOTNET_ROOT="$HOME/.dotnet"' >> ~/.bashrc
echo 'export PATH="$PATH:$HOME/.dotnet:$HOME/.dotnet/tools"' >> ~/.bashrc
sudo ln -s /root/.dotnet/dotnet /usr/bin/dotnet
source ~/.bashrc

# Проверьте установку
dotnet --info
dotnet --version
