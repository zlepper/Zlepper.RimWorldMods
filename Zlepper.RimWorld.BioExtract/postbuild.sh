RIMWORLD_PATH="C:/Program Files (x86)/Steam/steamapps/common/RimWorld"
MOD_FOLDER_PATH="${RIMWORLD_PATH}/Mods/Zlepper.RimWorld.BioExtract"

mkdir -p "${MOD_FOLDER_PATH}"
cp --recursive About/. "${MOD_FOLDER_PATH}/About"
cp --recursive Assemblies/. "${MOD_FOLDER_PATH}/Assemblies"
cp --recursive Defs/. "${MOD_FOLDER_PATH}/Defs"
cp --recursive Textures/. "${MOD_FOLDER_PATH}/Textures"

eval "'${RIMWORLD_PATH}/RimWorldWin64.exe'" --quickstart






