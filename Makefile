UNITY ?= /Applications/Unity/Hub/Editor/6000.4.5f1/Unity.app/Contents/MacOS/Unity
PROJECT_PATH := $(CURDIR)

.DEFAULT_GOAL := run

.PHONY: help setup prefabs wire normalize-sprites reprocess-sprites build run clean

help:
	@echo "Targets:"
	@echo "  make setup              - full bootstrap: scene, board, prefabs, wiring (idempotent)"
	@echo "  make prefabs            - create/refresh all prefabs (plants + zombies)"
	@echo "  make wire               - wire all prefabs into scene objects"
	@echo "  make normalize-sprites  - resize all sprite sheets to 200 px height via ffmpeg"
	@echo "  make reprocess-sprites  - re-slice sprites and rebuild .anim files in Unity"
	@echo "  make build              - compile and build the macOS app"
	@echo "  make run                - launch the built app"
	@echo "  make clean              - remove Builds/ output"

setup:
	$(UNITY) -batchmode -quit -nographics -projectPath "$(PROJECT_PATH)" -executeMethod ProjectSetup.FullSetup -logFile -

prefabs:
	$(UNITY) -batchmode -quit -nographics -projectPath "$(PROJECT_PATH)" -executeMethod PrefabFactory.CreateAndWireAllPrefabs -logFile -

wire:
	$(UNITY) -batchmode -quit -nographics -projectPath "$(PROJECT_PATH)" -executeMethod PrefabFactory.WireZombieSpawnerPrefab -logFile -

normalize-sprites:
	python3 scripts/normalize_sprites.py 200

reprocess-sprites:
	$(UNITY) -batchmode -quit -nographics -projectPath "$(PROJECT_PATH)" -executeMethod GifSpriteSheetProcessor.ProcessAll -logFile -

build:
	$(UNITY) -batchmode -quit -nographics -projectPath "$(PROJECT_PATH)" -executeMethod BuildScript.Build -logFile -

run:
	@open "Builds/PVZ.app"

clean:
	@rm -rf Builds
