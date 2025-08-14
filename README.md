# Unity LLM Dialogue Evaluation Tool

## 📑 Table of Contents
- [Introduction / Summary](#-introduction--summary)
- [Explanation of Scenes](#-explanation-of-scenes)
  - [Automated Testing Scene (Multi-LLM)](#1-automated-testing-scene-multi-llm)
  - [Automated Testing Scene (Individual)](#2-automated-testing-scene-individual)
  - [Single NPC Scene](#3-single-npc-scene)
  - [Individual NPC Scene](#4-individual-npc-scene)
  - [Quick Integration Package](#5-quick-integration-package)
- [Installation](#-installation)
- [Requirements](#-requirements)
- [Example Results](#-example-results)

## 📜 Introduction / Summary
This Unity-based tool is designed to evaluate and compare multiple Large Language Models (LLMs) in generating game dialogues in real time. It enables developers to integrate different LLMs into Unity, run automated or interactive tests, and visualise results through metrics such as **Coherence, Relevance, Naturalness, Engagement, Contextual Accuracy**, and **Average Response Time**.

The tool offers both **multi-LLM** and **single-LLM** testing environments, stress-testing capabilities, and ready-to-use scripts for quick integration into any Unity project. All results can be viewed in **graphs** and **tables**, allowing developers to make data-driven decisions when selecting the most suitable LLM for their game development needs.

## 🎥 Demo Video  
[Watch on YouTube](https://youtu.be/WXrxdSrgrxU)

---

## 🎮 Explanation of Scenes

### 1. Automated Testing Scene (Multi-LLM)
In this scene, **all integrated LLMs** generate a conversation simultaneously based on a shared prompt.
- Outputs are scored on all evaluation metrics.
- Average response times are calculated for each LLM.
- Results are displayed in **interactive graphs** and **sortable tables**.
- Useful for **side-by-side performance comparison** of multiple models.

---

### 2. Automated Testing Scene (Individual)
A stress-testing environment for a **single selected LLM**.
- Select the LLM from the dropdown.
- Runs up to **100 predefined test cases** stored in `Assets/Testcases/100Testcases.json`.
- Generates metric scores and average response time for each test case.
- Displays results in **graphs** and **tables** for performance trend analysis.
- Can view the result whenever needed.
---

### 3. Single NPC Scene
A controlled scenario with **4 characters** using a **single LLM (Gemini)**.
- The LLM creates the **entire conversation** in one request.
- Dialogues are then **assigned** to each character.
- Characters play the assigned lines in sequence.
- Great for **fast prototype testing** of multi-character dialogue flows.

---

### 4. Individual NPC Scene
An **advanced interactive environment** with **5 characters**, each powered by a **different LLM**.
- Dialogue is generated **turn-by-turn**.
- Includes **interruptions**, **moderator control**, and **player participation** options.
- Shows **metric scores** and **average response time** for each LLM after the session.

---

### 5. Quick Integration Package
A **standalone Tester Script** is provided as a Unity package.
- Import into any Unity project.
- Attach to agents or dialogue objects.
- Enter your LLM **Add API Key into Script**.
- Test with pre-coded prompts **immediately** without complex setup.
- Perfect for quick experiments or adding LLM dialogue capabilities to existing scenes.
![Screen Shot 2](https://github.com/safwanpaleri/CMP504_MasterProject/blob/main/SavedImage/Screenshot%202025-08-11%20152806.png?raw=true)
---

## 📦 Installation
1. Clone or download this repository.
2. Open the project in Unity **2022.3 LTS** or newer.
3. Add your LLM API keys in the **Inspector->LLM Testing Tool->API KEYS** or directly into script.
![Screenshot 1](https://github.com/safwanpaleri/CMP504_MasterProject/blob/main/SavedImage/Screenshot%202025-08-11%20152835.png?raw=true)
![ScreenShot 3](https://github.com/safwanpaleri/CMP504_MasterProject/blob/main/SavedImage/Screenshot%202025-08-11%20152937.png?raw=true)
5. Load any of the example scenes and press **Play**.

---

## 🛠️ Requirements
- Unity 2022.3 LTS (or higher)
- API keys for supported LLMs (e.g., OpenAI GPT, Gemini, Claude, Mistral, DeepSeek)
- Internet connection for API-based models

---

## 📊 Example Results
![Average Response Time - Mistral](https://github.com/safwanpaleri/CMP504_MasterProject/blob/main/SavedImage/Avg%20Response%20Time%20-%20Mistral.png?raw=true)
![Metric Score - Mistral](https://github.com/safwanpaleri/CMP504_MasterProject/blob/main/SavedImage/Metric%20Score%20-%20Mistral.png?raw=true)

