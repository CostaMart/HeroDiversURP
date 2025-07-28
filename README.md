# 🏜️ Dune Divers

**A university project developed for the "Video Games and Virtual Reality" course at the University of Sannio**  
Created by Costantino Martignetti and Alessandro Repola

---

## 🎮 Description

**Dune Divers** is a fast-paced third-person shooter set on a hostile desert planet. The player controls a futuristic armored soldier equipped with a jetpack and a high-powered blaster. The goal is to survive waves of alien enemies while leveraging rapid movement and modular gameplay upgrades.

The project was developed following a **Push-to-Data** paradigm: core gameplay components such as items, modifiers, and enemy behavior are fully **data-driven**, defined through external configuration files (e.g., JSON), improving modularity, maintainability, and extensibility.

> ⚠️ **Disclaimer**  
This game is **not a complete or polished product**, and may contain bugs or unfinished features.  
It was developed over **approximately three months**, while also attending other university courses and preparing for exams.  
At the start of the project, neither developer had any prior experience with **Unity** or other **game engines** — all skills were acquired during the course itself.  

The main goal was to demonstrate a solid application of the **Push-to-Data** development methodology — not to deliver a fully polished or commercial-grade game.  
Once a working and presentable version was completed, development was paused. However, the project is fully extensible and could be continued in the future.

---

## 🧪 Key Features

- 🎯 **Arcade-style third-person shooter** with fast strafe movement and reactive combat  
- 🌞 **Environmental overheating mechanic**: staying too long in direct sunlight disables the jetpack  
- 🧬 **Gameplay modifiers**: dynamically alter weapons and player behavior  
- 📁 **Data-driven configuration**: all items and modifiers are defined in external JSON files  
- 🔁 **High replayability** thanks to custom content support and modularity  
- 🤖 **Custom enemy AI** (the "Raim") built for flexibility and maintainability

---

## 🛠️ Tech Stack

- **Game Engine**: Unity  
- **Language**: C#  
- **Development Approach**: Push-to-Data  
- **Configuration Files**: JSON, Ad-hoc format

---

## 🚀 Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/dune-divers.git
   ```
2. Open the project in **Unity (recommended version: XX.X.XfX)**  
3. Load the main scene (e.g., `MainScene`) and click Play to run the game

> 📁 Check the `/Data` folder or `modifiers-schema.json` file for configuration examples.

---
if you just wish to try the game out, you can download the zip file posted as release of this repo.

## 📽️ Demo Video
#### Gampelay example
[![Gampelay example](https://img.youtube.com/vi/ev7MelVX60s/0.jpg)](https://www.youtube.com/watch?v=ev7MelVX60s)
#### Item system behaviour
[![Item system example](https://img.youtube.com/vi/fDJiObhR1MQ/0.jpg)](https://www.youtube.com/watch?v=fDJiObhR1MQ)


---




## 👨‍💻 Development Team

| Name             | Responsibilities                                    |
|------------------|----------------------------------------------------|
  | **Costantino Martignetti**                     | Items system, player controls |
| **Alessandro Repola** |  Enemy behavior, level design          |

---

## 📄 License

This project is licensed under the **MIT License** (or specify another if needed).  
Feel free to fork, contribute, or adapt the code.

---

## 🎓 Academic Context

This project was developed from scratch for the *"Video Games and Virtual Reality"* course at the **University of Sannio**, under the guidance of  
**Prof. Giovanni Caturano** and **Prof. Massimiliano Di Penta**.

The course aimed to teach students the **fundamentals of game design and development**, with a strong focus on modularity and external data configuration.  
The adopted **Push-to-Data** methodology allowed the game's core logic (e.g., modifiers, items, enemy behaviors) to be decoupled from code and defined via external JSON files.

All development took place over the course of about **three months**, during which the developers also attended other courses and had no prior experience with Unity.  
The team acquired hands-on knowledge of Unity, gameplay scripting, and modular architecture as part of this project.

> 🏅 The project was awarded the **highest possible grade: 30 with honors (30 e Lode)**.
