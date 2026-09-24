<template>
  <div class="landing-container">
    <!-- Hero Section -->
    <header class="hero-section">
      <div class="hero-content">
        <div class="hero-badge">
          <span class="badge-dot"></span>
          <span>NỀN TẢNG ÔN THI & FLASHCARD SRS</span>
        </div>

        <h1 class="hero-title">
          Ghi nhớ sâu sắc.<br />
          Ôn luyện bứt phá.
        </h1>

        <p class="hero-lead">
          Kết hợp thuật toán lặp lại ngắt quãng Spaced Repetition (SRS) với ngân hàng 2.254+ câu hỏi trắc nghiệm trường đại học và kỳ thi FE.
        </p>

        <div class="hero-cta-group">
          <RouterLink to="/quiz" class="btn-primary-glow">
            <i class="fa-solid fa-graduation-cap"></i>
            <span>Khám phá Ngân hàng Quiz</span>
          </RouterLink>

          <RouterLink to="/learn" class="btn-secondary-surface">
            <i class="fa-solid fa-layer-group"></i>
            <span>Vào Learn Lab</span>
          </RouterLink>

          <RouterLink to="/download" class="btn-ghost-link">
            <i class="fa-brands fa-windows"></i>
            <span>Tải Desktop & Extension</span>
          </RouterLink>
        </div>
      </div>

      <!-- Hero Visual Interactive Preview -->
      <div class="hero-preview-stage">
        <div class="interactive-preview-card glass-panel">
          <div class="preview-header">
            <div class="preview-badge-group">
              <span class="subject-tag">MLN122</span>
              <span class="subject-tag alt">FE Exam</span>
            </div>
            <span class="preview-counter">Câu 108 / 2.254</span>
          </div>

          <div class="preview-body">
            <p class="preview-question">
              Theo phép biện chứng duy vật, nguồn gốc và động lực của sự vận động, phát triển là gì?
            </p>

            <div class="preview-options">
              <div class="preview-option">
                <span class="opt-label">A</span>
                <span>Sự tác động của các yếu tố ngẫu nhiên từ môi trường</span>
              </div>
              <div class="preview-option is-correct">
                <span class="opt-label">B</span>
                <span>Mâu thuẫn biện chứng và sự đấu tranh giữa các mặt đối lập</span>
                <i class="fa-solid fa-check correct-icon"></i>
              </div>
              <div class="preview-option">
                <span class="opt-label">C</span>
                <span>Ý chí chủ quan của con người trong quá trình nhận thức</span>
              </div>
            </div>
          </div>

          <div class="preview-footer">
            <span class="srs-status">
              <i class="fa-solid fa-bolt text-warning me-1"></i>
              <span>Giải thích chi tiết & tự động ghi nhớ SRS</span>
            </span>
          </div>
        </div>
      </div>
    </header>

    <!-- Key Metrics Ribbon -->
    <section class="metrics-ribbon">
      <div class="metric-item">
        <strong>2.254+</strong>
        <span>Câu hỏi trắc nghiệm</span>
      </div>
      <div class="metric-divider"></div>
      <div class="metric-item">
        <strong>7 Môn</strong>
        <span>Ngân hàng đề thi thực chiến</span>
      </div>
      <div class="metric-divider"></div>
      <div class="metric-item">
        <strong>3 Nền tảng</strong>
        <span>Web · Windows · Extension</span>
      </div>
      <div class="metric-divider"></div>
      <div class="metric-item">
        <strong>100%</strong>
        <span>Local-first & Miễn phí</span>
      </div>
    </section>

    <!-- Popular Course Banks -->
    <section class="subject-showcase">
      <div class="section-heading">
        <h2>Ngân hàng câu hỏi theo môn học</h2>
        <p>Chọn môn học để bắt đầu luyện tập ngay lập tức mà không cần tài khoản.</p>
      </div>

      <div class="subject-grid">
        <RouterLink to="/quiz/mln122" class="subject-card">
          <div class="subject-header">
            <span class="subject-code">MLN122</span>
            <span class="subject-pill">781 câu</span>
          </div>
          <h3>Triết học Mác - Lênin</h3>
          <p>Trắc nghiệm toàn diện giáo trình, đề thi các kỳ và phân tích đáp án chi tiết.</p>
          <span class="card-action">Vào ôn tập &rarr;</span>
        </RouterLink>

        <RouterLink to="/quiz/prm393" class="subject-card">
          <div class="subject-header">
            <span class="subject-code accent-emerald">PRM393</span>
            <span class="subject-pill">450 câu</span>
          </div>
          <h3>Lập trình React Native</h3>
          <p>Câu hỏi trắc nghiệm thực hành di động, Hooks, Navigation, State Management.</p>
          <span class="card-action">Vào ôn tập &rarr;</span>
        </RouterLink>

        <RouterLink to="/quiz/jfe301" class="subject-card">
          <div class="subject-header">
            <span class="subject-code accent-purple">JFE301</span>
            <span class="subject-pill">512 câu</span>
          </div>
          <h3>Japanese for IT (FE 1)</h3>
          <p>Từ vựng thuật ngữ CNTT tiếng Nhật, ngữ cảnh phát triển phần mềm và đề FE.</p>
          <span class="card-action">Vào ôn tập &rarr;</span>
        </RouterLink>

        <RouterLink to="/quiz/jit401" class="subject-card">
          <div class="subject-header">
            <span class="subject-code accent-blue">JIT401</span>
            <span class="subject-pill">511 câu</span>
          </div>
          <h3>Japanese for IT (FE 2)</h3>
          <p>Giao tiếp dự án, giải thích yêu cầu nghiệp vụ và ôn thi kỹ sư CNTT tiêu chuẩn Nhật Bản.</p>
          <span class="card-action">Vào ôn tập &rarr;</span>
        </RouterLink>
      </div>
    </section>

    <!-- Core Capabilities Bento Grid -->
    <section class="capabilities-section">
      <div class="section-heading">
        <h2>Thiết kế cho việc học sâu</h2>
        <p>Không chỉ là làm đề trắc nghiệm, WordsNote là hệ thống hỗ trợ ghi nhớ bền vững.</p>
      </div>

      <div class="bento-grid">
        <!-- Tile 1: Large Featured -->
        <article class="bento-tile bento-feature-large">
          <div class="tile-icon-bubble">
            <i class="fa-solid fa-list-check"></i>
          </div>
          <h3>Chế độ Luyện Đề Thông Minh</h3>
          <p>
            Tự do chuyển đổi linh hoạt giữa <strong>Tất cả câu hỏi</strong>, <strong>Thứ tự ngẫu nhiên</strong>,
            <strong>Chỉ làm câu chưa hoàn thành</strong>, <strong>Ôn lại các câu trả lời sai</strong> và <strong>Câu đã đánh dấu sao</strong>.
            Bản đồ câu hỏi trực quan giúp theo dõi tiến độ từng phút.
          </p>
          <div class="bento-tags">
            <span>Ngẫu nhiên (Shuffle)</span>
            <span>Lọc câu sai</span>
            <span>Đánh dấu sao</span>
            <span>Bản đồ 1-click</span>
          </div>
        </article>

        <!-- Tile 2 -->
        <article class="bento-tile">
          <div class="tile-icon-bubble">
            <i class="fa-solid fa-repeat"></i>
          </div>
          <h3>Spaced Repetition (SRS)</h3>
          <p>
            Ứng dụng thuật toán ngắt quãng giúp tối ưu thời gian ôn tập. Từ vựng và câu hỏi khó sẽ xuất hiện thường xuyên hơn cho đến khi trở thành phản xạ.
          </p>
        </article>

        <!-- Tile 3 -->
        <article class="bento-tile">
          <div class="tile-icon-bubble">
            <i class="fa-solid fa-keyboard"></i>
          </div>
          <h3>Learn Lab Thực Hành</h3>
          <p>
            Chế độ gõ từ (type recall) với chuẩn hóa ký tự, flashcard 3D flip card và bài tập trắc nghiệm tự sinh từ chính bộ từ vựng cá nhân của bạn.
          </p>
        </article>

        <!-- Tile 4: Ecosystem -->
        <article class="bento-tile bento-ecosystem">
          <div class="tile-icon-bubble">
            <i class="fa-solid fa-cloud-arrow-up"></i>
          </div>
          <h3>Đa Nền Tảng: Web · Desktop · Extension</h3>
          <p>
            Ứng dụng Windows Desktop đóng gói MSIX (Microsoft Store) cho học tập ngoại tuyến; Extension Chromium hỗ trợ bôi đen tra từ và lưu trực tiếp vào bộ flashcard.
          </p>
          <div class="ecosystem-links">
            <RouterLink to="/download" class="eco-link">
              <i class="fa-brands fa-microsoft"></i>
              <span>Desktop App</span>
            </RouterLink>
            <RouterLink to="/download" class="eco-link">
              <i class="fa-brands fa-chrome"></i>
              <span>Chrome / Edge Extension</span>
            </RouterLink>
          </div>
        </article>
      </div>
    </section>

    <!-- Quick Start Flow -->
    <section class="quick-start-section">
      <div class="section-heading">
        <h2>3 bước bắt đầu ngay</h2>
      </div>

      <div class="flow-steps">
        <div class="flow-step">
          <div class="step-num">1</div>
          <h4>Chọn Ngân Hàng Hoặc Bộ Thẻ</h4>
          <p>Truy cập Quiz Banks để chọn môn học hoặc vào Manage để tự tạo bộ thẻ học từ vựng riêng.</p>
        </div>

        <div class="flow-step">
          <div class="step-num">2</div>
          <h4>Luyện Tập Hàng Ngày</h4>
          <p>Làm đề trắc nghiệm, bấm xem giải thích, đánh dấu sao câu khó và ôn lại danh sách câu sai.</p>
        </div>

        <div class="flow-step">
          <div class="step-num">3</div>
          <h4>Đồng Bộ Mọi Lúc Mọi Nơi</h4>
          <p>Đăng nhập Google khi muốn lưu phiên học trên đám mây và mở khóa toàn bộ bài học nâng cao.</p>
        </div>
      </div>
    </section>

    <!-- Final Call to Action -->
    <section class="final-cta glass-panel">
      <h2>Sẵn sàng bứt phá điểm số cùng WordsNote?</h2>
      <p>Trải nghiệm ngay hoàn toàn miễn phí trên Web, hoặc cài đặt Desktop App để học tập tập trung không gián đoạn.</p>
      <div class="final-cta-buttons">
        <RouterLink to="/quiz" class="btn-primary-glow">
          <i class="fa-solid fa-play"></i>
          <span>Bắt đầu Luyện Đề</span>
        </RouterLink>
        <RouterLink to="/download" class="btn-secondary-surface">
          <i class="fa-solid fa-download"></i>
          <span>Tải Ứng Dụng</span>
        </RouterLink>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { RouterLink } from 'vue-router'
</script>

<style scoped>
.landing-container {
  max-width: 1160px;
  margin: 0 auto;
  padding: 1.8rem 1.2rem 4rem;
  display: flex;
  flex-direction: column;
  gap: 3.5rem;
}

/* Hero Section */
.hero-section {
  display: grid;
  grid-template-columns: 1.15fr 0.95fr;
  align-items: center;
  gap: 2.5rem;
  padding: 1.5rem 0 1rem;
}

.hero-badge {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  background: var(--wn-primary-soft);
  color: var(--wn-primary);
  border: 1px solid color-mix(in srgb, var(--wn-primary) 25%, transparent);
  border-radius: 999px;
  padding: 0.35rem 0.85rem;
  font-size: 0.76rem;
  font-weight: 700;
  letter-spacing: 0.08em;
  margin-bottom: 1.2rem;
}

.badge-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: var(--wn-primary);
  box-shadow: 0 0 8px var(--wn-primary);
}

.hero-title {
  font-size: clamp(2.3rem, 3.8vw, 3.4rem);
  line-height: 1.12;
  font-weight: 800;
  letter-spacing: -0.02em;
  color: var(--wn-ink);
  margin: 0 0 1.2rem;
}

.hero-lead {
  font-size: 1.06rem;
  line-height: 1.6;
  color: var(--wn-muted);
  max-width: 54ch;
  margin: 0 0 2rem;
}

.hero-cta-group {
  display: flex;
  align-items: center;
  gap: 0.85rem;
  flex-wrap: wrap;
}

.btn-primary-glow {
  display: inline-flex;
  align-items: center;
  gap: 0.55rem;
  background: var(--wn-primary);
  color: var(--wn-on-primary);
  text-decoration: none;
  font-weight: 600;
  font-size: 0.96rem;
  padding: 0.72rem 1.35rem;
  border-radius: 14px;
  box-shadow: 0 4px 18px color-mix(in srgb, var(--wn-primary) 38%, transparent);
  transition: all 0.2s ease;
}

.btn-primary-glow:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 24px color-mix(in srgb, var(--wn-primary) 50%, transparent);
  color: var(--wn-on-primary);
}

.btn-secondary-surface {
  display: inline-flex;
  align-items: center;
  gap: 0.55rem;
  background: var(--wn-surface);
  color: var(--wn-ink);
  text-decoration: none;
  font-weight: 600;
  font-size: 0.96rem;
  padding: 0.72rem 1.25rem;
  border-radius: 14px;
  border: 1px solid var(--wn-border);
  box-shadow: var(--wn-shadow-soft);
  transition: all 0.2s ease;
}

.btn-secondary-surface:hover {
  background: var(--wn-primary-soft);
  border-color: color-mix(in srgb, var(--wn-primary) 35%, transparent);
  transform: translateY(-2px);
}

.btn-ghost-link {
  display: inline-flex;
  align-items: center;
  gap: 0.45rem;
  color: var(--wn-muted);
  text-decoration: none;
  font-size: 0.92rem;
  font-weight: 500;
  padding: 0.5rem 0.75rem;
  transition: color 0.18s ease;
}

.btn-ghost-link:hover {
  color: var(--wn-primary);
}

/* Interactive Card Preview */
.hero-preview-stage {
  perspective: 1000px;
}

.interactive-preview-card {
  background: var(--wn-surface);
  border: 1px solid var(--wn-border);
  border-radius: 24px;
  padding: 1.8rem;
  box-shadow: var(--wn-shadow-soft);
  position: relative;
  transition: transform 0.3s ease;
}

.interactive-preview-card:hover {
  transform: translateY(-4px);
}

.preview-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1.1rem;
}

.preview-badge-group {
  display: flex;
  gap: 0.4rem;
}

.subject-tag {
  background: var(--wn-primary-soft);
  color: var(--wn-primary);
  font-weight: 700;
  font-size: 0.74rem;
  padding: 0.2rem 0.55rem;
  border-radius: 6px;
}

.subject-tag.alt {
  background: rgba(245, 158, 11, 0.12);
  color: #d97706;
}

.preview-counter {
  font-size: 0.78rem;
  color: var(--wn-muted);
  font-weight: 500;
}

.preview-question {
  font-size: 1.04rem;
  line-height: 1.5;
  font-weight: 700;
  color: var(--wn-ink);
  margin: 0 0 1.2rem;
}

.preview-options {
  display: flex;
  flex-direction: column;
  gap: 0.65rem;
}

.preview-option {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.65rem 0.85rem;
  border-radius: 12px;
  background: var(--wn-surface-soft);
  border: 1px solid var(--wn-border);
  font-size: 0.88rem;
  color: var(--wn-ink);
  position: relative;
}

.opt-label {
  font-weight: 700;
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 6px;
  background: var(--wn-surface);
  border: 1px solid var(--wn-border);
  font-size: 0.76rem;
  color: var(--wn-muted);
  flex-shrink: 0;
}

.preview-option.is-correct {
  background: color-mix(in srgb, #10b981 12%, var(--wn-surface));
  border-color: #10b981;
  color: var(--wn-ink);
  font-weight: 600;
}

.preview-option.is-correct .opt-label {
  background: #10b981;
  color: #fff;
  border-color: #10b981;
}

.correct-icon {
  margin-left: auto;
  color: #10b981;
  font-size: 0.95rem;
}

.preview-footer {
  margin-top: 1.2rem;
  padding-top: 0.85rem;
  border-top: 1px solid var(--wn-border);
  font-size: 0.8rem;
  color: var(--wn-muted);
}

/* Metrics Ribbon */
.metrics-ribbon {
  display: flex;
  align-items: center;
  justify-content: space-around;
  padding: 1.2rem 1.8rem;
  border-radius: 20px;
  background: var(--wn-surface);
  border: 1px solid var(--wn-border);
  box-shadow: var(--wn-shadow-soft);
  flex-wrap: wrap;
  gap: 1.2rem;
}

.metric-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.25rem;
  text-align: center;
}

.metric-item strong {
  font-size: 1.6rem;
  font-weight: 800;
  color: var(--wn-primary);
  line-height: 1.1;
}

.metric-item span {
  font-size: 0.84rem;
  color: var(--wn-muted);
}

.metric-divider {
  width: 1px;
  height: 38px;
  background: var(--wn-border);
}

/* Sections Shared Heading */
.section-heading {
  text-align: center;
  max-width: 650px;
  margin: 0 auto 2rem;
}

.section-heading h2 {
  font-size: 1.95rem;
  line-height: 1.2;
  font-weight: 800;
  margin: 0 0 0.55rem;
  color: var(--wn-ink);
}

.section-heading p {
  font-size: 0.98rem;
  color: var(--wn-muted);
  line-height: 1.5;
  margin: 0;
}

/* Subject Showcase */
.subject-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 1.2rem;
}

.subject-card {
  text-decoration: none;
  background: var(--wn-surface);
  border: 1px solid var(--wn-border);
  border-radius: 20px;
  padding: 1.5rem;
  box-shadow: var(--wn-shadow-soft);
  display: flex;
  flex-direction: column;
  transition: all 0.22s ease;
}

.subject-card:hover {
  transform: translateY(-4px);
  border-color: color-mix(in srgb, var(--wn-primary) 40%, transparent);
}

.subject-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 0.85rem;
}

.subject-code {
  font-size: 0.82rem;
  font-weight: 800;
  letter-spacing: 0.05em;
  padding: 0.25rem 0.6rem;
  border-radius: 8px;
  background: var(--wn-primary-soft);
  color: var(--wn-primary);
}

.subject-code.accent-emerald {
  background: rgba(16, 185, 129, 0.12);
  color: #059669;
}

.subject-code.accent-purple {
  background: rgba(168, 85, 247, 0.12);
  color: #7c3aed;
}

.subject-code.accent-blue {
  background: rgba(14, 165, 233, 0.12);
  color: #0284c7;
}

.subject-pill {
  font-size: 0.76rem;
  color: var(--wn-muted);
  font-weight: 600;
}

.subject-card h3 {
  font-size: 1.18rem;
  font-weight: 700;
  color: var(--wn-ink);
  margin: 0 0 0.5rem;
}

.subject-card p {
  font-size: 0.86rem;
  line-height: 1.45;
  color: var(--wn-muted);
  margin: 0 0 1.2rem;
  flex-grow: 1;
}

.card-action {
  font-size: 0.85rem;
  font-weight: 700;
  color: var(--wn-primary);
  margin-top: auto;
}

/* Bento Grid */
.bento-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1.2rem;
}

.bento-tile {
  background: var(--wn-surface);
  border: 1px solid var(--wn-border);
  border-radius: 22px;
  padding: 1.8rem;
  box-shadow: var(--wn-shadow-soft);
  display: flex;
  flex-direction: column;
}

.bento-feature-large {
  grid-column: span 2;
  background:
    radial-gradient(120% 120% at 100% 0%, color-mix(in srgb, var(--wn-primary) 10%, transparent), transparent 60%),
    var(--wn-surface);
}

.bento-ecosystem {
  grid-column: span 3;
  display: grid;
  grid-template-columns: 1.2fr 1fr;
  align-items: center;
  gap: 2rem;
}

.tile-icon-bubble {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  background: var(--wn-primary-soft);
  color: var(--wn-primary);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.15rem;
  margin-bottom: 1.1rem;
}

.bento-tile h3 {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--wn-ink);
  margin: 0 0 0.6rem;
}

.bento-tile p {
  font-size: 0.9rem;
  line-height: 1.55;
  color: var(--wn-muted);
  margin: 0 0 1rem;
}

.bento-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 0.45rem;
  margin-top: auto;
}

.bento-tags span {
  font-size: 0.76rem;
  padding: 0.25rem 0.65rem;
  border-radius: 999px;
  background: var(--wn-surface-soft);
  border: 1px solid var(--wn-border);
  color: var(--wn-ink);
  font-weight: 500;
}

.ecosystem-links {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.eco-link {
  text-decoration: none;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 1rem;
  background: var(--wn-surface-soft);
  border: 1px solid var(--wn-border);
  border-radius: 14px;
  color: var(--wn-ink);
  font-weight: 600;
  font-size: 0.9rem;
  transition: all 0.18s ease;
}

.eco-link i {
  font-size: 1.25rem;
  color: var(--wn-primary);
}

.eco-link:hover {
  background: var(--wn-primary-soft);
  border-color: color-mix(in srgb, var(--wn-primary) 35%, transparent);
}

/* Quick Start Flow */
.flow-steps {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1.5rem;
}

.flow-step {
  background: var(--wn-surface);
  border: 1px solid var(--wn-border);
  border-radius: 20px;
  padding: 1.8rem;
  position: relative;
  box-shadow: var(--wn-shadow-soft);
}

.step-num {
  width: 38px;
  height: 38px;
  border-radius: 10px;
  background: var(--wn-primary);
  color: var(--wn-on-primary);
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  font-size: 1rem;
  margin-bottom: 1.1rem;
}

.flow-step h4 {
  font-size: 1.05rem;
  font-weight: 700;
  color: var(--wn-ink);
  margin: 0 0 0.5rem;
}

.flow-step p {
  font-size: 0.88rem;
  line-height: 1.5;
  color: var(--wn-muted);
  margin: 0;
}

/* Final CTA */
.final-cta {
  background:
    radial-gradient(120% 120% at 50% 0%, color-mix(in srgb, var(--wn-primary) 18%, transparent), transparent 70%),
    var(--wn-surface);
  border: 1px solid var(--wn-border);
  border-radius: 28px;
  padding: 3rem 2rem;
  text-align: center;
  box-shadow: var(--wn-shadow-soft);
}

.final-cta h2 {
  font-size: 2rem;
  font-weight: 800;
  margin: 0 0 0.8rem;
  color: var(--wn-ink);
}

.final-cta p {
  font-size: 1rem;
  color: var(--wn-muted);
  max-width: 60ch;
  margin: 0 auto 1.8rem;
  line-height: 1.5;
}

.final-cta-buttons {
  display: flex;
  justify-content: center;
  gap: 1rem;
  flex-wrap: wrap;
}

@media (max-width: 900px) {
  .hero-section {
    grid-template-columns: 1fr;
  }

  .bento-grid {
    grid-template-columns: 1fr;
  }

  .bento-feature-large,
  .bento-ecosystem {
    grid-column: span 1;
  }

  .bento-ecosystem {
    grid-template-columns: 1fr;
  }

  .metrics-ribbon {
    display: grid;
    grid-template-columns: 1fr 1fr;
  }

  .metric-divider {
    display: none;
  }
}
</style>
