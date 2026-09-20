#!/usr/bin/env node
/**
 * Automated Publisher for Microsoft Edge Add-ons
 * Uses Microsoft Edge Add-ons Publish API v1.1
 */

import fs from 'node:fs'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const rootDir = path.resolve(__dirname, '..')

// Simple .env parser (Ponytail style - zero dependency)
function loadEnv() {
  const envPath = path.join(rootDir, '.env')
  if (!fs.existsSync(envPath)) return {}
  const content = fs.readFileSync(envPath, 'utf8')
  const env = {}
  for (const line of content.split('\n')) {
    const trimmed = line.trim()
    if (!trimmed || trimmed.startsWith('#')) continue
    const eqIdx = trimmed.indexOf('=')
    if (eqIdx !== -1) {
      const key = trimmed.slice(0, eqIdx).trim()
      const val = trimmed.slice(eqIdx + 1).trim()
      env[key] = val
    }
  }
  return env
}

const env = { ...loadEnv(), ...process.env }

const clientId = env.EDGE_CLIENT_ID
const apiKey = env.EDGE_API_KEY
let productId = env.EDGE_PRODUCT_ID || process.argv[2]

console.log('=== Microsoft Edge Add-ons Automated Publisher ===\n')

if (!clientId || !apiKey) {
  console.error('❌ Error: Missing EDGE_CLIENT_ID or EDGE_API_KEY in .env file.')
  process.exit(1)
}

if (!productId) {
  console.log('ℹ️  No Product ID specified.')
  console.log('--------------------------------------------------')
  console.log('LƯU Ý CỦA MICROSOFT:')
  console.log('1. Lần đầu tiên phát hành extension lên Microsoft Edge Add-ons,')
  console.log('   Microsoft bắt buộc bạn phải tải lên file zip và điền thông tin qua giao diện web.')
  console.log('2. File zip đã được tạo sẵn tại:')
  console.log('   release/microsoft-edge-addon/WordsNote-Edge-Addon-v1.1.2.zip')
  console.log('3. Truy cập: https://partner.microsoft.com/dashboard/microsoftedge')
  console.log('   - Nhấn "Create new extension" và tải file zip trên lên.')
  console.log('   - Sau khi tạo xong, Microsoft sẽ cấp cho bạn một "Product ID"')
  console.log('     (hoặc bạn có thể thấy nó trên thanh URL: /products/{productId}/...)')
  console.log('4. Điền Product ID vào file .env (EDGE_PRODUCT_ID=...)')
  console.log('   hoặc chạy: node scripts/publish-edge-addon.mjs <product-id>')
  console.log('   để script tự động publish toàn bộ các bản update tiếp theo!')
  console.log('--------------------------------------------------')
  process.exit(0)
}

const packageZipPath = path.join(rootDir, 'release', 'microsoft-edge-addon', 'WordsNote-Edge-Addon-v1.1.2.zip')

if (!fs.existsSync(packageZipPath)) {
  console.error(`❌ Error: Package zip file not found at: ${packageZipPath}`)
  console.log('Please run "npm run package:edge" inside src/extension first.')
  process.exit(1)
}

const BASE_URL = 'https://api.addons.microsoftedge.microsoft.com/v1.1'

async function uploadPackage() {
  console.log(`📦 Uploading package to Edge Add-ons (Product ID: ${productId})...`)
  const packageStream = fs.readFileSync(packageZipPath)

  const uploadUrl = `${BASE_URL}/products/${productId}/submissions/draft/package`
  const res = await fetch(uploadUrl, {
    method: 'POST',
    headers: {
      Authorization: `ApiKey ${apiKey}`,
      'X-ClientID': clientId,
      'Content-Type': 'application/zip',
    },
    body: packageStream,
  })

  if (!res.ok) {
    const errorText = await res.text()
    throw new Error(`Upload failed (${res.status} ${res.statusText}): ${errorText}`)
  }

  const operationLocation = res.headers.get('Location')
  console.log(`✅ Upload started successfully! Operation: ${operationLocation || 'Pending'}`)
  return operationLocation
}

async function waitForOperation(operationUrl) {
  if (!operationUrl) return
  console.log('⏳ Checking package processing status...')

  for (let i = 0; i < 30; i++) {
    await new Promise((r) => setTimeout(r, 4000))
    const res = await fetch(operationUrl, {
      method: 'GET',
      headers: {
        Authorization: `ApiKey ${apiKey}`,
        'X-ClientID': clientId,
      },
    })

    if (!res.ok) {
      console.warn(`Status check returned ${res.status}, retrying...`)
      continue
    }

    const data = await res.json()
    console.log(`   Status: ${data.status || 'InProgress'}`)

    if (data.status === 'Succeeded') {
      console.log('✅ Package processing succeeded!')
      return
    }

    if (data.status === 'Failed') {
      throw new Error(`Package processing failed: ${JSON.stringify(data.errors || data)}`)
    }
  }

  throw new Error('Timeout waiting for package processing.')
}

async function publishSubmission() {
  console.log('🚀 Triggering publication on Microsoft Edge Add-ons...')
  const publishUrl = `${BASE_URL}/products/${productId}/submissions`

  const res = await fetch(publishUrl, {
    method: 'POST',
    headers: {
      Authorization: `ApiKey ${apiKey}`,
      'X-ClientID': clientId,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      notes: 'Automated release via WordsNote Publish API',
    }),
  })

  if (!res.ok) {
    const errorText = await res.text()
    throw new Error(`Publish trigger failed (${res.status} ${res.statusText}): ${errorText}`)
  }

  console.log('🎉 Submission published successfully! The extension is now in Microsoft review queue.')
}

async function run() {
  try {
    const opUrl = await uploadPackage()
    if (opUrl) {
      await waitForOperation(opUrl)
    }
    await publishSubmission()
  } catch (err) {
    console.error(`\n❌ Failed: ${err.message}`)
    process.exit(1)
  }
}

run()
