I probably made this more complicated than it needed to be.

Set your git server ip address in request.sh and run.

```
dotnet build; GETVARS=$(dotnet run <token>); eval $GETVARS; ./create_server.sh $USERNAME "$SSHKEY" $REPOPATH; ./request.sh $PUSHTOKEN; sudo -u $USERNAME git clone /home/$USERNAME/$REPOPATH /home/$USERNAME/repo; SOLUTION=$(sudo -u $USERNAME cat /home/$USERNAME/repo/solution.txt); dotnet run <token> $SOLUTION;
```