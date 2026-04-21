-include .env
export

APP_NAME     := TorchKeeper
PROJECT      := TorchKeeper/TorchKeeper.csproj
FRAMEWORK    := net10.0-maccatalyst
OUTPUT_DIR   := TorchKeeper/bin/Release/$(FRAMEWORK)
APP_BUNDLE   := $(OUTPUT_DIR)/$(APP_NAME).app
ZIP_FILE     := $(OUTPUT_DIR)/$(APP_NAME).zip
ENTITLEMENTS := TorchKeeper/Entitlements.plist

# Guard: fail early if required env vars are missing
check-env:
	@test -n "$(APPLE_ID)"          || (echo "ERROR: APPLE_ID is not set"; exit 1)
	@test -n "$(TEAM_ID)"           || (echo "ERROR: TEAM_ID is not set"; exit 1)
	@test -n "$(APP_PASSWORD)"      || (echo "ERROR: APP_PASSWORD is not set"; exit 1)
	@test -n "$(SIGNING_IDENTITY)"  || (echo "ERROR: SIGNING_IDENTITY is not set"; exit 1)

build:
	dotnet publish $(PROJECT) \
	  -f $(FRAMEWORK) \
	  -c Release \
	  -p:CreatePackage=false

sign: check-env
	codesign --deep --force --verify --verbose \
	  --options runtime \
	  --entitlements "$(ENTITLEMENTS)" \
	  --sign "$(SIGNING_IDENTITY)" \
	  "$(APP_BUNDLE)"

zip:
	cd $(OUTPUT_DIR) && zip -r $(APP_NAME).zip $(APP_NAME).app

notarize: check-env
	xcrun notarytool submit "$(ZIP_FILE)" \
	  --apple-id "$(APPLE_ID)" \
	  --team-id "$(TEAM_ID)" \
	  --password "$(APP_PASSWORD)" \
	  --wait

staple:
	xcrun stapler staple "$(APP_BUNDLE)"
	cd $(OUTPUT_DIR) && zip -r $(APP_NAME).zip $(APP_NAME).app

release: build sign zip notarize staple
	@echo "Done: $(ZIP_FILE)"

.PHONY: check-env build sign zip notarize staple release
