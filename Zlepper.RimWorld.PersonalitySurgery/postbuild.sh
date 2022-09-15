RIMWORLD_PATH="C:/Program Files (x86)/Steam/steamapps/common/RimWorld"
MOD_NAME="Zlepper.RimWorld.PersonalitySurgery"
MOD_FOLDER_PATH="$RIMWORLD_PATH/Mods/$MOD_NAME"

rm -rf "${MOD_FOLDER_PATH}"
mkdir -p "${MOD_FOLDER_PATH}"
cp --recursive "$MOD_NAME/About/". "${MOD_FOLDER_PATH}/About"
cp --recursive "Output/PersonalitySurgery/Assemblies/." "${MOD_FOLDER_PATH}/Assemblies"
cp --recursive "$MOD_NAME/Defs/". "${MOD_FOLDER_PATH}/Defs"
cp --recursive "$MOD_NAME/Languages/". "${MOD_FOLDER_PATH}/Languages"

eval "'${RIMWORLD_PATH}/RimWorldWin64.exe'" --quickstart






