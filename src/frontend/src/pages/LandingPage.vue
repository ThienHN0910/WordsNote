<template>
  <section class="landing-wrap">
    <div class="hero-card">
      <p class="eyebrow">WordsNote Studio</p>
      <h1>Learn in a way that feels handcrafted, not generic.</h1>
      <p class="lead">
        Explore flashcards and practice mode without signing in. Sign in only when you want to manage
        your own collections and cards.
      </p>
      <div class="hero-actions">
        <RouterLink class="cta primary" to="/learn">Start Learning</RouterLink>
        <RouterLink class="cta ghost" to="/manage">Manage Collections</RouterLink>
      </div>
    </div>

    <div class="feature-grid">
      <article>
        <h2>Flashcards</h2>
        <p>Flip, focus, and move at your own pace with distraction-free cards.</p>
      </article>
      <article>
        <h2>Learn Mode</h2>
        <p>Type answers and get instant feedback with normalized text comparison.</p>
      </article>
      <article>
        <h2>Practice Mode</h2>
        <p>Challenge yourself with quick multiple-choice rounds and score tracking.</p>
      </article>
    </div>
  </section>
</template>

<script setup lang="ts">
import { RouterLink } from 'vue-router'
</script>

<style scoped>
.landing-wrap {
  max-width: 1120px;
  margin: 0 auto;
  padding: 2.2rem 1rem 3rem;
  display: grid;
  gap: 1.4rem;
}

.hero-card {
  border-radius: 28px;
  padding: 2.2rem;
  color: #1f2133;
  border: 1px solid #e8e1d7;
  background:
    radial-gradient(140% 120% at 0% 0%, rgba(255, 188, 119, 0.3), transparent 45%),
    radial-gradient(90% 120% at 100% 100%, rgba(100, 173, 255, 0.22), transparent 50%),
    #fffaf2;
}

.eyebrow {
  margin: 0 0 0.5rem;
  text-transform: uppercase;
  font-size: 0.74rem;
  letter-spacing: 0.18em;
  color: #7c5f3e;
}

h1 {
  font-size: clamp(2.1rem, 3vw, 3rem);
  line-height: 1.08;
  margin: 0;
}

.lead {
  max-width: 70ch;
  margin-top: 0.95rem;
  color: #3b4357;
}

.hero-actions {
  margin-top: 1.3rem;
  display: flex;
  gap: 0.8rem;
  flex-wrap: wrap;
}

.cta {
  text-decoration: none;
  padding: 0.7rem 1rem;
  border-radius: 12px;
  border: 1px solid transparent;
  font-weight: 600;
}

.cta.primary {
  background: #1f4ed8;
  color: #fff;
}

.cta.ghost {
  background: rgba(255, 255, 255, 0.55);
  border-color: #cad3e6;
  color: #1d315f;
}

.feature-grid {
  display: grid;
  gap: 0.9rem;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
}

.feature-grid article {
  padding: 1.1rem;
  border-radius: 18px;
  border: 1px solid #ebe8e1;
  background: #fff;
}

.feature-grid h2 {
  margin: 0 0 0.45rem;
  font-size: 1.12rem;
}

.feature-grid p {
  margin: 0;
  color: #4f5566;
}
</style>
