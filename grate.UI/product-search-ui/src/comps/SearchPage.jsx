import { useState, useEffect } from "react";
import axios from "axios";
import "./SearchPage.css";

function PaginationBar({
    pageNo,
    totalPages,
    loading,
    onPageChange
}) {
    return (
        <div className="pagination-bar">
            <button
                className="page-btn"
                onClick={() => onPageChange(pageNo - 1)}
                disabled={pageNo === 1 || loading}
            >
                ← Prev
            </button>

            <span className="page-indicator">
                Page <strong>{pageNo}</strong> of <strong>{totalPages}</strong>
            </span>

            <button
                className="page-btn"
                onClick={() => onPageChange(pageNo + 1)}
                disabled={pageNo >= totalPages || loading}
            >
                Next →
            </button>
        </div>
    );
}

export default function SearchPage() {
    const [search, setSearch] = useState("");
    const [products, setProducts] = useState([]);
    const [loading, setLoading] = useState(false);
    const [pageNo, setPageNo] = useState(1);
    const [totalPages, setTotalPages] = useState(0);

    // ✅ NEW STATES (Infinite Scroll)
    const [isInfiniteScroll, setIsInfiniteScroll] = useState(false);
    const [lastScrollPage, setLastScrollPage] = useState(1);

    const pageSize = 300;

    const handleSearch = async (page = 1, append = false) => {
        if (!search.trim()) {
            setProducts([]);
            setTotalPages(0);
            return;
        }

        try {
            setLoading(true);

            const response = await axios.get(
                "http://localhost:5050/api/product/searchoptimA",
                {
                    params: {
                        search,
                        pageNo: page,
                        pageSize
                    }
                }
            );

            // ✅ append or replace
            if (append) {
                setProducts(prev => [...prev, ...response.data.items]);
            } else {
                setProducts(response.data.items);
            }

            setTotalPages(
                Math.ceil(
                    response.data.totalCount /
                    response.data.pageSize
                )
            );

            setPageNo(response.data.pageNo);
        }
        catch (error) {
            console.log(error);
            console.log(error.response);
            console.log(error.message);
        }
        finally {
            setLoading(false);
        }
    };

    const handleKeyDown = (e) => {
        if (e.key === "Enter") {
            setLastScrollPage(1);
            handleSearch(1, false);
        }
    };

    // ✅ Infinite Scroll Listener (only when enabled)
    useEffect(() => {
        if (!isInfiniteScroll) return;

        const handleScroll = () => {
            const scrollTop = window.scrollY;
            const windowHeight = window.innerHeight;
            const fullHeight = document.documentElement.scrollHeight;

            if (
                scrollTop + windowHeight >= fullHeight - 100 &&
                !loading
            ) {
                const nextPage = lastScrollPage + 1;

                if (nextPage <= totalPages) {
                    setLastScrollPage(nextPage);
                    handleSearch(nextPage, true);
                }
            }
        };

        window.addEventListener("scroll", handleScroll);
        return () => window.removeEventListener("scroll", handleScroll);

    }, [isInfiniteScroll, loading, lastScrollPage, totalPages, search]);

    return (
        <div className="page">

            {/* ✅ TOGGLE BUTTON (NEW) */}
            <div className="toggle-container">
                <button
                    className={`toggle-btn ${isInfiniteScroll ? "active" : ""}`}
                    onClick={() => {
                        setIsInfiniteScroll(prev => !prev);
                        setProducts([]);
                        setPageNo(1);
                        setLastScrollPage(1);
                    }}
                >
                    {isInfiniteScroll
                        ? "Infinite Scroll: ON"
                        : "Infinite Scroll: OFF"}
                </button>
            </div>

            {/* SEARCH BAR */}
            <div className={products.length === 0 ? "hero" : "hero-top"}>
                <h1 className="logo">Product Search</h1>

                <div className="search-container">
                    <input
                        type="text"
                        placeholder="Search products..."
                        value={search}
                        onChange={(e) => {
                            setSearch(e.target.value);
                            setPageNo(1);
                        }}
                        onKeyDown={handleKeyDown}
                        className="search-input"
                    />

                    <button
                        className="search-button"
                        onClick={() => {
                            setLastScrollPage(1);
                            handleSearch(1, false);
                        }}
                    >
                        Search
                    </button>
                </div>
            </div>

            {/* LOADING */}
            {loading && (
                <div className="loading">
                    Searching...
                </div>
            )}

            {/* RESULTS */}
            <div className="results">
                {products.map(product => (
                    <div
                        key={product.id}
                        className="product-card"
                    >
                        <h3>{product.name}</h3>

                        <p className="description">
                            {product.description}
                        </p>

                        <div className="product-footer">
                            <span>
                                ₹{product.price.toLocaleString()}
                            </span>

                            <span>
                                Stock: {product.stockQuantity}
                            </span>
                        </div>
                    </div>
                ))}
            </div>

            {/* PAGINATION (ONLY WHEN INFINITE SCROLL OFF) */}
            {!isInfiniteScroll && totalPages > 0 && (
                <PaginationBar
                    pageNo={pageNo}
                    totalPages={totalPages}
                    loading={loading}
                    onPageChange={(page) => {
                        setLastScrollPage(page);
                        handleSearch(page, false);
                    }}
                />
            )}
        </div>
    );
}