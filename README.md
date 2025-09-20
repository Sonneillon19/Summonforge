# Turn‑Based RPG Starter (Unity 6000.1.14f1)

Proyecto base para un RPG táctico por turnos inspirado en Summoners War.

## Requisitos
- Unity **6000.1.14f1**
- Git + Git LFS
- (Opcional) Configurar **UnityYAMLMerge** como mergetool para resolver conflictos en YAML.

## Configuración en Unity (importante)
1. **Edit → Project Settings → Editor**
   - **Version Control:** *Visible Meta Files*
   - **Asset Serialization:** *Force Text*
2. (Opcional) Activa *Enter Play Mode Options* para iteraciones más rápidas.

## Estructura sugerida
```
Assets/
  Scenes/
  Scripts/
    Battle/
    Data/
    Meta/
  Art/
  Audio/
```
> *No subas **Library/**, **Temp/**, **Build/** al repo. Lo maneja `.gitignore`.*

## Primer uso (nuevo repo)
```bash
git init
git branch -M main
git lfs install
git remote add origin <URL-de-tu-repo>
git add .gitattributes .gitignore README.md
git commit -m "chore: initial Git/LFS config (Unity 6000.1.14f1)"
git push -u origin main
```

## Proyecto existente
Coloca estos archivos en la raíz del proyecto Unity y confírmalos.

## UnityYAMLMerge (opcional pero recomendado)
Configura la herramienta de merge de Unity para archivos YAML (prefabs, escenas, etc.). Ajusta la ruta según tu instalación:

**Windows (ejemplo):**
```
git config merge.tool unityyamlmerge
git config mergetool.unityyamlmerge.cmd ""C:/Program Files/Unity/Hub/Editor/6000.1.14f1/Editor/Data/Tools/UnityYAMLMerge.exe" merge -p "$BASE" "$REMOTE" "$LOCAL" "$MERGED""
```

**macOS (ejemplo):**
```
git config merge.tool unityyamlmerge
git config mergetool.unityyamlmerge.cmd '"/Applications/Unity/Hub/Editor/6000.1.14f1/Unity.app/Contents/Tools/UnityYAMLMerge" merge -p "$BASE" "$REMOTE" "$LOCAL" "$MERGED"'
```

## Branching sugerido
- `main`: estable / releases.
- `develop`: integración continua.
- `feature/<nombre>`: trabajo de features.
- `hotfix/<id>`: arreglos sobre `main`.

¡Listo! Clona en cualquier equipo con la misma versión de Unity y abre el proyecto.
