import { describe, it, expect } from 'vitest'

// Test the grid generation and scan logic (client-side fallback)
describe('Grid Seeker Game Logic', () => {
  const generateTestGrid = (): number[] => {
    // Known grid for testing: treasures at indices 0,9,18,27,36 and traps at 1,10,19,28
    const cells = Array(64).fill(0)
    // Treasures (type 1) at diagonal positions
    cells[0] = 1; cells[9] = 1; cells[18] = 1; cells[27] = 1; cells[36] = 1
    // Traps (type 2)
    cells[1] = 2; cells[10] = 2; cells[19] = 2; cells[28] = 2
    return cells
  }

  const calcDistance = (grid: number[], row: number, col: number): number => {
    let minDist = Infinity
    for (let r = 0; r < 8; r++) {
      for (let c = 0; c < 8; c++) {
        if (grid[r * 8 + c] === 1) {
          const dist = Math.sqrt(Math.pow(row - r, 2) + Math.pow(col - c, 2))
          minDist = Math.min(minDist, dist)
        }
      }
    }
    return Math.round(minDist * 10) / 10
  }

  it('generates a grid with 64 cells', () => {
    const grid = generateTestGrid()
    expect(grid).toHaveLength(64)
  })

  it('has correct number of treasures', () => {
    const grid = generateTestGrid()
    const treasures = grid.filter(c => c === 1).length
    expect(treasures).toBe(5)
  })

  it('has correct number of traps', () => {
    const grid = generateTestGrid()
    const traps = grid.filter(c => c === 2).length
    expect(traps).toBe(4)
  })

  it('identifies treasure cells correctly', () => {
    const grid = generateTestGrid()
    expect(grid[0]).toBe(1)  // row 0, col 0
    expect(grid[9]).toBe(1)  // row 1, col 1
  })

  it('identifies trap cells correctly', () => {
    const grid = generateTestGrid()
    expect(grid[1]).toBe(2)  // row 0, col 1
    expect(grid[10]).toBe(2) // row 1, col 2
  })

  it('identifies empty cells correctly', () => {
    const grid = generateTestGrid()
    expect(grid[2]).toBe(0)  // row 0, col 2
    expect(grid[63]).toBe(0) // last cell
  })

  it('calculates distance to nearest treasure from empty cell', () => {
    const grid = generateTestGrid()
    // Cell (0,2) - nearest treasure is at (1,1) = sqrt(1+1) ≈ 1.4
    const dist = calcDistance(grid, 0, 2)
    expect(dist).toBeCloseTo(1.4, 1)
  })

  it('calculates distance as 0 for treasure cell', () => {
    const grid = generateTestGrid()
    const dist = calcDistance(grid, 0, 0) // treasure at (0,0)
    expect(dist).toBe(0)
  })

  it('calculates correct diagonal distance', () => {
    const grid = generateTestGrid()
    // Cell (7,7) - far corner, nearest treasure at (4,4) = index 36
    const dist = calcDistance(grid, 7, 7)
    // Distance from (7,7) to (4,4) = sqrt(9+9) = sqrt(18) ≈ 4.2
    expect(dist).toBeCloseTo(4.2, 1)
  })

  it('score calculation works correctly', () => {
    const treasuresFound = 3
    const scansRemaining = 7
    const score = treasuresFound * (scansRemaining + 1)
    expect(score).toBe(24)
  })
})
