#!/bin/bash

USERNAME=$1
SSHKEY=$2
REPOPATH=$3

sudo useradd -m $USERNAME

USERROOTDIR=/home/$USERNAME

sudo -u $USERNAME mkdir $USERROOTDIR/.ssh && sudo chmod 700 $USERROOTDIR/.ssh
sudo -u $USERNAME touch $USERROOTDIR/.ssh/authorized_keys && sudo chmod 600 $USERROOTDIR/.ssh/authorized_keys

sudo bash -c "echo '$SSHKEY' >> $USERROOTDIR/.ssh/authorized_keys"

sudo -u $USERNAME mkdir -p $USERROOTDIR/$REPOPATH
sudo -u $USERNAME git init $USERROOTDIR/$REPOPATH --bare
