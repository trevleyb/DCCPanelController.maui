PPDIR="$HOME/Library/Developer/Xcode/UserData/Provisioning Profiles"
PPFILE=$(ls "$PPDIR" | grep -i "8bea3875-3785-4e51-8ced-a4bc09c8f345" | head -n1)
echo "Profile file: $PPDIR/$PPFILE"
echo "== Basic info =="
security cms -D -i "$PPDIR/$PPFILE" | /usr/bin/plutil -p - | egrep 'Name|UUID|TeamIdentifier|ApplicationIdentifier|ExpirationDate|Entitlements'

echo "== Which certificates does this profile allow? =="
security cms -D -i "$PPDIR/$PPFILE" \
| /usr/bin/plutil -extract DeveloperCertificates raw -o - - \
| openssl pkcs7 -inform DER -print_certs -text -noout \
| egrep 'Subject: CN=|Issuer: CN='

echo "== What signing identities are in your login keychain? =="
security find-identity -v -p codesigning login.keychain-db

echo "== What signing profiles can we find =="
    for dir in "$HOME/Library/MobileDevice/Provisioning Profiles" "$HOME/Library/Developer/Xcode/UserData/Provisioning Profiles"; do
      [[ -d "$dir" ]] || continue
      for f in "$dir"/*.mobileprovision; do
        [[ -e "$f" ]] || continue
        uuid=$(basename "$f" .mobileprovision)
        name=$(security cms -D -i "$f" | /usr/bin/plutil -extract Name xml1 -o - - | /usr/bin/plutil -p - 2>/dev/null | sed 's/^"//;s/"$//')
        echo "$dir -> UUID: $uuid | Name: $name"
      done
    done