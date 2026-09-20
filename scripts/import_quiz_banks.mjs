import fs from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const QUIZ_BANKS_DIR = path.resolve(__dirname, '../src/backend/FeatureFusion/Data/QuizBanks');

function formatExplanation(explanation) {
  if (!explanation) return null;
  if (typeof explanation === 'string') return explanation.trim();

  if (typeof explanation === 'object') {
    const parts = [];
    if (explanation.questionVi) parts.push(`📌 Dịch câu hỏi: ${explanation.questionVi}`);
    if (explanation.answerDisplay) parts.push(`✅ Đáp án: ${explanation.answerDisplay}`);
    if (explanation.concept) parts.push(`💡 Khái niệm & Kiến thức:\n${explanation.concept}`);
    if (explanation.whyCorrect) parts.push(`🎯 Tại sao đúng:\n${explanation.whyCorrect}`);
    if (explanation.whyWrong && typeof explanation.whyWrong === 'object') {
      const wrongList = Object.entries(explanation.whyWrong)
        .map(([k, v]) => `• [${k}]: ${v}`)
        .join('\n');
      if (wrongList) {
        parts.push(`🔍 Phân tích các phương án khác:\n${wrongList}`);
      }
    }
    return parts.join('\n\n');
  }

  return null;
}

function normalizeAnswers(rawAnswer) {
  if (Array.isArray(rawAnswer)) {
    return rawAnswer.map(a => String(a).trim().toUpperCase()).filter(Boolean);
  }
  if (typeof rawAnswer === 'string') {
    // e.g. "A, B" or "A" or "A,B"
    const cleaned = rawAnswer.replace(/;/g, ',').split(',').map(s => s.trim().toUpperCase()).filter(Boolean);
    return cleaned.length > 0 ? cleaned : [rawAnswer.trim().toUpperCase()];
  }
  return ['A'];
}

async function fetchSubjectData(url) {
  console.log(`Fetching ${url}...`);
  const res = await fetch(url);
  if (!res.ok) {
    throw new Error(`Failed to fetch ${url}: ${res.statusText}`);
  }
  const code = await res.text();
  const window = {};
  // Safely execute JavaScript data file containing window.QUIZ_DATA
  const fn = new Function('window', code);
  fn(window);
  const data = Object.values(window.QUIZ_DATA || {})[0];
  if (!Array.isArray(data)) {
    throw new Error(`Invalid data array in ${url}`);
  }
  return data;
}

function transformQuestions(rawList, subjectCode) {
  return rawList.map((item, index) => {
    const id = index + 1;
    const answers = normalizeAnswers(item.answer || item.answers);
    const choose = item.chooseType && item.chooseType.toLowerCase().includes('all')
      ? answers.length
      : (answers.length > 1 ? answers.length : 1);

    let questionText = item.question || `Câu hỏi số ${id}`;
    if (item.image) {
      questionText = `${questionText}\n\n![Đề thi](${item.image})`;
    }

    return {
      id,
      question: questionText,
      options: item.options || {},
      answers,
      choose,
      note: null,
      explanation: formatExplanation(item.explanation),
      source: item.source || item.taskLabel || subjectCode,
      exam: item.taskLabel || item.task || `${subjectCode}_FE`
    };
  });
}

async function main() {
  const targets = [
    {
      id: 'prn232',
      code: 'PRN232',
      title: 'Lập trình .NET & Web API (FE SP26, B5 FE, FA25 & PE)',
      color: '#0284c7',
      isRestricted: false,
      url: 'https://albazzz.github.io/FPT/quiz/data/prn232.js'
    },
    {
      id: 'ite302c',
      code: 'ITE302c',
      title: 'Ethics in Information Technology (12 đề FE SP24-SP26)',
      color: '#e11d48',
      isRestricted: false,
      url: 'https://albazzz.github.io/FPT/quiz/data/ite.js'
    },
    {
      id: 'hcm202',
      code: 'HCM202',
      title: 'Tư tưởng Hồ Chí Minh (15 bộ đề FE SU24-SU26)',
      color: '#d97706',
      isRestricted: false,
      url: 'https://albazzz.github.io/FPT/quiz/data/hcm202.js'
    }
  ];

  await fs.mkdir(QUIZ_BANKS_DIR, { recursive: true });

  const catalogFile = path.join(QUIZ_BANKS_DIR, 'catalog.json');
  let catalog = { title: 'Ôn tập FE – FuExam', subjects: [], sources: [] };
  if (await fs.stat(catalogFile).catch(() => null)) {
    catalog = JSON.parse(await fs.readFile(catalogFile, 'utf8'));
  }

  for (const target of targets) {
    console.log(`\nProcessing ${target.code}...`);
    const rawQuestions = await fetchSubjectData(target.url);
    const questions = transformQuestions(rawQuestions, target.code);

    const subjectFileContent = {
      title: `${target.code} – ${target.title}`,
      code: `${target.code}_ALL`,
      subject: target.code,
      total: questions.length,
      answered: questions.length,
      questions
    };

    const targetJsonPath = path.join(QUIZ_BANKS_DIR, `${target.id}.json`);
    await fs.writeFile(targetJsonPath, JSON.stringify(subjectFileContent, null, 2), 'utf8');
    console.log(`Saved ${questions.length} questions to ${targetJsonPath}`);

    // Update catalog.json
    const existingIndex = catalog.subjects.findIndex(s => s.id === target.id);
    const subjectEntry = {
      id: target.id,
      code: target.code,
      name: target.title,
      file: `data/${target.id}.json`,
      total: questions.length,
      color: target.color,
      isRestricted: target.isRestricted
    };

    if (existingIndex >= 0) {
      catalog.subjects[existingIndex] = subjectEntry;
    } else {
      catalog.subjects.push(subjectEntry);
    }
  }

  await fs.writeFile(catalogFile, JSON.stringify(catalog, null, 2), 'utf8');
  console.log(`\nCatalog successfully updated at ${catalogFile}`);
}

main().catch(err => {
  console.error('Ingestion failed:', err);
  process.exit(1);
});
