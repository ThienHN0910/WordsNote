declare module 'pdfjs-dist/build/pdf.mjs' {
  export const GlobalWorkerOptions: {
    workerSrc: string
  }

  export function getDocument(src: string | Uint8Array): {
    promise: Promise<{
      numPages: number
      getPage: (pageNumber: number) => Promise<{
        getViewport: (options: { scale: number }) => { width: number; height: number }
        render: (renderContext: Record<string, unknown>) => { promise: Promise<void> }
      }>
    }>
  }
}
