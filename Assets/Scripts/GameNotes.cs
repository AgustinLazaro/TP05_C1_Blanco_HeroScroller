/*
 * Notas generales del juego (versión actual).
 * Referencia rápida de interacción entre sistemas y nombres esperados en la jerarquía.
 */

/*
 * UIManager.cs
 * ------------
 * Menú principal:
 * - Paneles: PanelTitle, PanelOptions, PanelCredits.
 * - Botones: ButtonStart, ButtonOptions, ButtonCredits, ButtonBackOptions, ButtonBackCredits
 *   (fallback de back: "ButtonBack" dentro de cada panel).
 * - Música de menú: si 'menuMusic' está asignado, se reproduce SOLO en la escena de menú.
 * - ESC en menú: deshabilitado (la pausa solo existe in-game).
 *
 * In-Game:
 * - Pausa: PanelPause con botones ButtonResume y ButtonPauseOptions.
 * - Tecla de pausa: configurable (por defecto P) y ESC (solo in-game).
 * - Al pausar: Time.timeScale = 0; al reanudar: Time.timeScale = 1.
 * - Opciones desde Pausa usan el mismo PanelOptions del menú. El botón Back en opciones vuelve a PanelPause
 *   si viniste desde la pausa; de lo contrario, vuelve a PanelTitle.
 *
 * Opciones (volumen):
 * - Sliders: SliderMaster, SliderMusic, SliderSFX -> AudioManager.MasterVolume/MusicVolume/SfxVolume.
 * - Persistencia PlayerPrefs: "vol_master", "vol_music", "vol_sfx".
 */

/*
 * GameManager.cs
 * --------------
 * Estado global de objetivos y HUD (solo TMP):
 * - Objetivos configurables en Inspector: targetCoins, targetKills (0 = no requerido).
 * - Progreso: coinsCollected, enemiesKilled.
 * - HUD TMP: "TextCoin" o "TextCoins" para monedas; "TextEnemy" o "TextKills" para kills.
 * - API pública:
 *   - AddCoin(): suma 1 moneda y evalúa victoria.
 *   - EnemyKilled(): suma 1 kill y evalúa victoria.
 * - Condición de victoria: (coinsCollected >= targetCoins si targetCoins>0) y (enemiesKilled >= targetKills si targetKills>0).
 *   Si se cumple -> VictoryUI.Show(resumen) y Time.timeScale = 0.
 */

/*
 * VictoryUI.cs
 * ------------
 * - Panel: PanelVictory (hijo del HUD), botones: ButtonRetry, ButtonMenu, texto TMP opcional: TextVictory.
 * - Al mostrar:
 *   - Detiene la música de fondo (AudioManager.StopMusic) y reproduce SFX de victoria (victorySfx) si está asignado.
 *   - Activa PanelVictory y pone Time.timeScale = 0.
 * - ButtonRetry: recarga la escena actual (Time.timeScale = 1).
 * - ButtonMenu: carga SceneMenu (Time.timeScale = 1).
 */

/*
 * GameOverUI.cs
 * -------------
 * - Panel: PanelGameOver (hijo del HUD), botones: ButtonRetry, ButtonMenu.
 * - Al mostrar:
 *   - Detiene la música de fondo (AudioManager.StopMusic) y reproduce SFX de derrota (defeatSfx) si está asignado.
 *   - Activa PanelGameOver y pone Time.timeScale = 0.
 * - ButtonRetry: recarga la escena actual (Time.timeScale = 1).
 * - ButtonMenu: carga SceneMenu (Time.timeScale = 1).
 */

/*
 * AudioManager.cs
 * ---------------
 * - Singleton persistente con dos AudioSource: música (loop) y SFX (one-shot).
 * - Volúmenes:
 *   - MasterVolume -> AudioListener.volume (PlayerPrefs "vol_master").
 *   - MusicVolume  -> musicSource.volume  (PlayerPrefs "vol_music").
 *   - SfxVolume    -> sfxSource.volume    (PlayerPrefs "vol_sfx").
 * - API:
 *   - PlayMusic(clip, loop=true), StopMusic().
 *   - PlaySFX(clip) (respeta SfxVolume, suena aunque Time.timeScale = 0).
 */

/*
 * HUD (nombres esperados)
 * -----------------------
 * - Textos TMP: "TextCoin"/"TextCoins" para monedas; "TextEnemy"/"TextKills" para kills.
 * - Barra de vida del jugador: "HP player bar" (Slider) con fill para color.
 * - Paneles: PanelPause, PanelOptions, PanelCredits, PanelTitle, PanelVictory, PanelGameOver.
 * - EventSystem presente en escena.
 */

/*
 * Pickable.cs
 * -----------
 * - Tipos: Coin, HealthPowerUp, InvincibilityPowerUp, DoubleJumpPowerUp.
 * - Monedas: cuentan por unidad -> GameManager.AddCoin().
 * - Power-ups: restauran vida (porcentaje), invencibilidad temporal, doble salto temporal.
 * - Respawn: solo para power-ups (no para monedas), en área [spawnAreaMin..spawnAreaMax].
 */

/*
 * Enemy.cs
 * --------
 * - Vida: health/maxHealth; daño al jugador via PlayerController/PlayerHealth.
 * - Barra de vida por enemigo (prefab con Slider) instanciada en Canvas Overlay, y posicionada en pantalla
 *   con offset worldYOffset. Se destruye al morir el enemigo.
 * - Al morir: GameManager.EnemyKilled() y Destroy(enemy).
 */

/*
 * PlayerHealth.cs
 * ---------------
 * - Salud del jugador; actualiza Slider y color del fill (verde ↔ rojo).
 * - Invencibilidad temporal (corrutina); mientras dura, ignora daño.
 * - Al llegar a 0: deshabilita PlayerController, muestra GameOverUI y pausa.
 */

/*
 * PlayerController.cs
 * -------------------
 * - Movimiento, salto (con opción de doble salto temporal) y disparo (Bullet).
 * - Daño en colisiones con enemigos: delega a PlayerHealth.TakeDamage().
 */

/*
 * Bullet.cs
 * ---------
 * - Movimiento hacia la dirección apuntada; al colisionar con Enemy aplica daño y se destruye.
 * - Se destruye al salir de la vista de cámara.
 */

/*
 * UI/Flujo general
 * ----------------
 * - Menú principal:
 *   - Música de menú (opcional), botones para Play/Options/Credits y Back en cada subpanel.
 *   - La tecla ESC no abre la pausa en el menú.
 * - In-Game:
 *   - Pausa con P/ESC (PanelPause). Desde ahí se accede a Opciones; Back vuelve a Pausa.
 *   - Victoria/Derrota: panel correspondiente, SFX dedicado, música detenida, Time.timeScale = 0.
 */

/*
 * Recomendaciones de escena
 * -------------------------
 * - Canvas del HUD en Screen Space - Overlay.
 * - PanelVictory/PanelGameOver/PanelPause como hijos del HUD.
 * - Asegurar EventSystem en escena.
 * - Configurar targetCoins/targetKills en GameManager según el nivel.
 */