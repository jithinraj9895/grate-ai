import { useState } from "react";
import axios from "axios";
import "./SemanticSearchBar.css";

export default function SemanticSearchBar({ onResults }) {
    const [query, setQuery] = useState("");
    const [loading, setLoading] = useState(false);

    const search = async () => {
        if (!query.trim()) return;

        try {
            setLoading(true);

            const response = await axios.get(
                "http://localhost:5050/api/product/semantic",
                {
                    params: {
                        search: query,
                        pageNo: 1,
                        pageSize: 10
                    }
                }
            );

            onResults(response.data.Items);
        }
        catch (err) {
            console.error(err);
        }
        finally {
            setLoading(false);
        }
    };

    return (
        <div className="semantic-search">
            <input
                type="text"
                placeholder="Semantic Search..."
                value={query}
                onChange={(e) => setQuery(e.target.value)}
                onKeyDown={(e) => {
                    if (e.key === "Enter") search();
                }}
            />

            <button
                onClick={search}
                disabled={loading}
            >
                {loading ? "..." : "Semantic"}
            </button>
        </div>
    );
}