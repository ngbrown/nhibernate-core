# docker Firebird

## Default password for `sysdba`
The default password for `sysdba` is randomly generated when you first launch the container, 
look in the docker log for your container or pull /var/firebird/etc/SYSDBA.password.
Alternatively you may pass the environment variable ISC_PASSWORD to set the default password.

## Description
This is a Firebird SQL Database container.

## Default Login information
Username: SYSDBA
Password is either set by `ISC_PASSWORD` or randomized

## Environment Variables:
### `TZ`
TimeZone. (i.e. America/Chicago)

### `ISC_PASSWORD`
Default `sysdba` user password, if left blank a random 20 character password will be set instead.
The password used will be placed in /var/firebird/etc/SYSDBA.password.
If a random password is generated then it will be in the log for the container.

### `FIREBIRD_DATABASE`
If this is set then a database will be created with this name under the `/databases` volume with the 'UTF8'
default character set and if `FIREBIRD_USER` is also set then `FIREBIRD_USER` will be given ownership.

### `FIREBIRD_USER`
This user will be created and given ownership of `FIREBIRD_DATABASE`.
This variable is only used if `FIREBIRD_DATABASE` is also set.

### `FIREBIRD_PASSWORD`
The password for `FIREBIRD_USER`, if left blank a random 20 character password will be set instead.
If a random password is generated then it will be in the log for the container.

### `<VARIABLE>_FILE`
If set to the path to a file then the named variable minus the _FILE portion will contain the contents of that file.
This is useful for using docker secrets to manage your password.
This applies to all variables except `TZ`

## Volumes:

### `/databases/`
Default location to put database files

### `/var/firebird/run`
guardian lock DIR

### `/var/firebird/etc`
config files DIR
message files DIR

### `/var/firebird/log`
log files DIR

### `/var/firebird/system`
security database DIR

### `/tmp/firebird`
Database lock directory

## Exposes: 
### 3050/tcp

## Events
Please note for events to work properly you must either configure RemoteAuxPort and forward it with -p using a direct mapping where both sides internal and external use the same port or use --net=host to allow the random port mapping to work.
see: http://www.firebirdfaq.org/faq53/ for more information on event port mapping.
