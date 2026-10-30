const canvas = document.getElementById('gameCanvas');
const ctx = canvas.getContext('2d');
const scoreEl = document.getElementById('score');
const bestEl = document.getElementById('best');
const startButton = document.getElementById('startButton');

const groundY = canvas.height - 52;
const lanePositions = [canvas.width * 0.28, canvas.width * 0.5, canvas.width * 0.72];

const state = {
  running: false,
  gameOver: false,
  score: 0,
  best: Number(localStorage.getItem('templeBest')) || 0,
  speed: 5.4,
  distance: 0,
  spawnTimer: 0,
  lastTime: 0,
  player: {
    lane: 1,
    x: lanePositions[1],
    y: groundY - 52,
    width: 34,
    height: 52,
    jumpVelocity: 0,
    onGround: true,
    color: '#f5d884',
    outline: '#ad7d18',
  },
  obstacles: [],
  particles: [],
};

bestEl.textContent = state.best;

function resetGame() {
  state.running = false;
  state.gameOver = false;
  state.score = 0;
  state.distance = 0;
  state.speed = 5.4;
  state.spawnTimer = 0;

  state.player.lane = 1;
  state.player.x = lanePositions[1];
  state.player.y = groundY - state.player.height;
  state.player.jumpVelocity = 0;
  state.player.onGround = true;

  state.obstacles = [];
  state.particles = [];

  updateHud();
}

function updateHud() {
  scoreEl.textContent = Math.floor(state.score);
  bestEl.textContent = state.best;
}

function startGame() {
  resetGame();
  state.running = true;
  startButton.textContent = 'Restart Run';
}

function setBest() {
  if (state.score > state.best) {
    state.best = Math.floor(state.score);
    localStorage.setItem('templeBest', String(state.best));
  }
  bestEl.textContent = state.best;
}

function laneChange(direction) {
  if (!state.running || state.gameOver) return;

  const nextLane = Math.min(2, Math.max(0, state.player.lane + direction));
  if (nextLane !== state.player.lane) {
    state.player.lane = nextLane;
  }
}

function jump() {
  if (!state.running || state.gameOver) return;

  if (state.player.onGround) {
    state.player.onGround = false;
    state.player.jumpVelocity = -15.2;
  }
}

function createObstacle() {
  const lane = Math.floor(Math.random() * 3);
  const height = 42 + Math.random() * 36;
  const width = 34 + Math.random() * 18;
  const obstacle = {
    lane,
    x: lanePositions[lane],
    y: -height - 30,
    width,
    height,
    color: '#7a4e36',
    outline: '#2e190f',
  };

  if (Math.random() > 0.7) {
    obstacle.color = '#d7b84d';
    obstacle.outline = '#785b16';
  }

  state.obstacles.push(obstacle);
}

function updatePlayer() {
  const player = state.player;
  const gravity = 0.72;

  player.x = lanePositions[player.lane];
  player.jumpVelocity += gravity;
  player.y += player.jumpVelocity;

  if (player.y >= groundY - player.height) {
    player.y = groundY - player.height;
    player.jumpVelocity = 0;
    player.onGround = true;
  }
}

function updateObstacles() {
  const speedBoost = 0.0022;
  state.speed += speedBoost;

  for (const obstacle of state.obstacles) {
    obstacle.y += state.speed * 2.7;
  }

  state.obstacles = state.obstacles.filter((obstacle) => obstacle.y < canvas.height + 150);
}

function createDust(x, y, amount = 3) {
  for (let i = 0; i < amount; i += 1) {
    state.particles.push({
      x,
      y,
      radius: 2 + Math.random() * 5,
      dx: (Math.random() - 0.5) * 2.5,
      dy: (Math.random() - 0.5) * 2.2,
      life: 18 + Math.random() * 16,
      color: 'rgba(255, 230, 160, 0.45)',
    });
  }
}

function updateParticles() {
  state.particles.forEach((particle) => {
    particle.x += particle.dx;
    particle.y += particle.dy;
    particle.life -= 1;
  });

  state.particles = state.particles.filter((particle) => particle.life > 0);
}

function checkCollisions() {
  const player = state.player;

  for (const obstacle of state.obstacles) {
    const sameLane = obstacle.lane === player.lane;
    const playerBottom = player.y + player.height;
    const obstacleLeftX = obstacle.x - obstacle.width / 2;
    const obstacleRightX = obstacle.x + obstacle.width / 2;
    const playerLeftX = player.x - player.width / 2;
    const playerRightX = player.x + player.width / 2;

    const xOverlap = playerRightX > obstacleLeftX && playerLeftX < obstacleRightX;
    const yOverlap = playerBottom > obstacle.y && player.y < obstacle.y + obstacle.height;

    if (sameLane && xOverlap && yOverlap) {
      state.running = false;
      state.gameOver = true;
      setBest();
      createDust(player.x, player.y + player.height / 2, 18);
      return;
    }
  }
}

function updateScore() {
  if (!state.running) return;

  state.score += 0.5;
  state.distance += state.speed * 0.16;

  if (state.score > state.best) {
    state.best = Math.floor(state.score);
    bestEl.textContent = state.best;
  }

  if (Math.floor(state.score) % 25 === 0) {
    createDust(canvas.width * 0.85, groundY - 14, 4);
  }
}

function updateGame(delta) {
  if (!state.running) return;

  state.spawnTimer += delta;
  if (state.spawnTimer > Math.max(650, 1500 - state.score * 4.5)) {
    createObstacle();
    state.spawnTimer = 0;
  }

  updatePlayer();
  updateObstacles();
  checkCollisions();
  updateParticles();
  updateScore();
  updateHud();
}

function drawBackground() {
  ctx.clearRect(0, 0, canvas.width, canvas.height);

  const skyGradient = ctx.createLinearGradient(0, 0, 0, canvas.height);
  skyGradient.addColorStop(0, '#2e5b7d');
  skyGradient.addColorStop(0.42, '#1a455e');
  skyGradient.addColorStop(0.42, '#486f39');
  skyGradient.addColorStop(1, '#1e3a24');
  ctx.fillStyle = skyGradient;
  ctx.fillRect(0, 0, canvas.width, canvas.height);

  for (let i = 0; i < 16; i += 1) {
    const x = (i * 62 + (state.distance * 0.8) % 70) % (canvas.width + 80);
    const hillY = canvas.height - 100 - (i % 3) * 18;
    ctx.fillStyle = 'rgba(22, 41, 28, 0.38)';
    ctx.beginPath();
    ctx.moveTo(x, canvas.height);
    ctx.quadraticCurveTo(x + 35, hillY, x + 70, canvas.height);
    ctx.fill();
  }

  ctx.fillStyle = '#3e2d1b';
  ctx.fillRect(0, groundY, canvas.width, canvas.height - groundY);

  for (let i = 0; i < 6; i += 1) {
    const laneX = lanePositions[i % 3];
    ctx.strokeStyle = 'rgba(255, 224, 147, 0.25)';
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.moveTo(laneX, groundY);
    ctx.lineTo(laneX, canvas.height);
    ctx.stroke();
  }

  const groundPattern = 30;
  for (let x = -20; x < canvas.width + 30; x += groundPattern) {
    const offset = ((state.distance * 1.3) % groundPattern);
    ctx.fillStyle = 'rgba(255, 199, 92, 0.18)';
    ctx.fillRect(x - offset, groundY + 12, 18, 4);
  }
}

function drawPlayer() {
  const player = state.player;
  const x = player.x;
  const y = player.y;

  ctx.fillStyle = '#0c1f1b';
  ctx.fillRect(x - player.width / 2 + 6, y + 8, player.width - 12, 12);

  ctx.fillStyle = player.color;
  ctx.fillRect(x - player.width / 2, y + 14, player.width, player.height - 14);

  ctx.fillStyle = '#f7eca4';
  ctx.fillRect(x - 10, y + 22, 8, 10);
  ctx.fillRect(x + 2, y + 22, 8, 10);

  ctx.fillStyle = '#1b1b1b';
  ctx.fillRect(x - 8, y + 30, 5, 10);
  ctx.fillRect(x + 3, y + 30, 5, 10);

  ctx.fillStyle = player.outline;
  ctx.fillRect(x - player.width / 2, y + 14, 5, player.height - 14);
  ctx.fillRect(x + player.width / 2 - 5, y + 14, 5, player.height - 14);

  if (!player.onGround) {
    ctx.fillStyle = 'rgba(255, 238, 180, 0.35)';
    ctx.beginPath();
    ctx.arc(x, y + 18, 12, 0, Math.PI * 2);
    ctx.fill();
  }
}

function drawObstacles() {
  for (const obstacle of state.obstacles) {
    const x = obstacle.x;
    const y = obstacle.y;

    ctx.fillStyle = obstacle.color;
    ctx.fillRect(x - obstacle.width / 2, y, obstacle.width, obstacle.height);

    ctx.fillStyle = obstacle.outline;
    ctx.fillRect(x - obstacle.width / 2, y, 6, obstacle.height);
    ctx.fillRect(x + obstacle.width / 2 - 6, y, 6, obstacle.height);

    ctx.fillStyle = 'rgba(255, 232, 170, 0.2)';
    ctx.fillRect(x - obstacle.width / 2 + 8, y + 8, obstacle.width - 16, obstacle.height * 0.18);
  }
}

function drawParticles() {
  state.particles.forEach((particle) => {
    ctx.fillStyle = particle.color;
    ctx.beginPath();
    ctx.arc(particle.x, particle.y, particle.radius, 0, Math.PI * 2);
    ctx.fill();
  });
}

function drawGameOver() {
  if (!state.gameOver) return;

  ctx.fillStyle = 'rgba(6, 8, 12, 0.5)';
  ctx.fillRect(0, 0, canvas.width, canvas.height);

  ctx.fillStyle = '#f6ebc3';
  ctx.textAlign = 'center';
  ctx.font = 'bold 42px Arial';
  ctx.fillText('Run Over', canvas.width / 2, canvas.height / 2 - 8);

  ctx.font = 'bold 24px Arial';
  ctx.fillText(`Score: ${Math.floor(state.score)}`, canvas.width / 2, canvas.height / 2 + 36);
  ctx.fillText('Press Start Run to play again', canvas.width / 2, canvas.height / 2 + 74);
}

function render() {
  drawBackground();
  drawParticles();
  drawObstacles();
  drawPlayer();
  drawGameOver();
}

let lastTimestamp = 0;
function gameLoop(timestamp) {
  const delta = timestamp - lastTimestamp;
  lastTimestamp = timestamp;

  updateGame(delta);
  render();

  requestAnimationFrame(gameLoop);
}

window.addEventListener('keydown', (event) => {
  const key = event.key.toLowerCase();

  if (key === 'arrowleft' || key === 'a') {
    laneChange(-1);
  } else if (key === 'arrowright' || key === 'd') {
    laneChange(1);
  } else if (key === 'arrowup' || key === 'w' || key === ' ') {
    event.preventDefault();
    jump();
  } else if (key === 'enter' && state.gameOver) {
    startGame();
  }
});

canvas.addEventListener('pointerdown', () => {
  if (!state.running) {
    startGame();
    return;
  }

  const rect = canvas.getBoundingClientRect();
  const clickX = (event.clientX - rect.left) / rect.width;

  if (clickX < 0.5) {
    laneChange(-1);
  } else {
    laneChange(1);
  }
});

startButton.addEventListener('click', () => {
  startGame();
});

resetGame();
requestAnimationFrame(gameLoop);
