RIMWORLD_PATH="C:/Program Files (x86)/Steam/steamapps/common/RimWorld"
MOD_FOLDER_PATH="${RIMWORLD_PATH}/Mods/Zlepper.RimWorld.FastFarm"

mkdir -p "${MOD_FOLDER_PATH}"
cp --recursive About/. "${MOD_FOLDER_PATH}/About"
cp --recursive Assemblies/. "${MOD_FOLDER_PATH}/Assemblies"

eval "'${RIMWORLD_PATH}/RimWorldWin64.exe'"






